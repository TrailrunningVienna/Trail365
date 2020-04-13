using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Trail365.Data;
using Trail365.Internal;

namespace Trail365.Tasks
{
    public abstract class BackgroundTask
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialize"></param>
        /// <returns>a not queued Background Task</returns>
        public BackgroundTask ContinueWith<T>(Action<T> initialize) where T : BackgroundTask, new()
        {
            var task = BackgroundTaskFactory.CreateTask<T>(this.Context.ServiceScopeFactory, this.Context.Url, this.QueueLogger);
            if (initialize != null)
            {
                initialize(task);
            }
            this.Continuation = () =>
            {
                IBackgroundTaskQueue _queue = this.Context.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
                task.Queue(_queue);
            };
            return task;
        }

        public string Caption { get; set; }

        internal ILogger QueueLogger = NullLogger.Instance;

        public BackgroundTaskContext Context { get; set; }

        protected virtual void OnBeforeExecute()
        { }

        private Action Continuation;

        protected void HandleContinuations()
        {
            if (this.Continuation != null)
            {
                this.Continuation();
            }
        }

        protected abstract Task Execute(CancellationToken cancellationToken);

        public void Queue(IBackgroundTaskQueue queue, bool disabledLogging = false)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            if (this.Context == null)
            {
                throw new InvalidOperationException("Context must be set");
            }
            queue.QueueBackgroundWorkItem(this.CreateWorkItemForQueue(this.Context, this.QueueLogger, disabledLogging));
        }

        private static readonly EventId TaskStarted = new EventId(300, nameof(TaskStarted));
        private static readonly EventId TaskSuccess = new EventId(301, nameof(TaskSuccess));
        private static readonly EventId TaskError = new EventId(301, nameof(TaskError));

        private Func<CancellationToken, Task> CreateWorkItemForQueue(BackgroundTaskContext context, ILogger logger, bool logDisabled)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (logger == null) throw new InvalidOperationException(nameof(logger));

            Func<CancellationToken, Task> result = async (cts) =>
            {
                using (var scope = context.ServiceScopeFactory.CreateScope())
                {
                    context.ServiceProvider = scope.ServiceProvider;

                    string currentTaskName = this.GetType().Name;

                    using (var loggerFactory = new LoggerFactory())
                    {
                        context.LoggerFactory = loggerFactory;
                        var dbContext = context.ServiceProvider.GetRequiredService<TaskContext>();
                        using (BackgroundTaskLoggingProvider loggerProvider = new BackgroundTaskLoggingProvider(dbContext, logDisabled))
                        {
                            loggerFactory.AddProvider(loggerProvider);
                            var engineLogger = loggerFactory.CreateLogger(currentTaskName); //WM 03/2020 logging with the final category => user context!
                            try
                            {
                                this.OnBeforeExecute();
                            }
                            catch (Exception ex)
                            {
                                engineLogger.LogError(TaskError, ex, nameof(this.OnBeforeExecute));
                                logger.LogError(TaskError, ex, nameof(this.OnBeforeExecute));
                                return;
                            }

                            context.DefaultLogger = engineLogger;

                            string suffix = string.Empty;

                            if (!string.IsNullOrEmpty(this.Caption))
                            {
                                suffix = $"{this.Caption}";
                            }

                            try
                            {
                                engineLogger.LogTrace(TaskStarted, $"Task started {suffix}".Trim() + ".");
                                loggerProvider.Flush(); //start must be visible immediately
                                var sw = Stopwatch.StartNew();
                                await this.Execute(cts);
                                sw.Stop();
                                engineLogger.LogInformation(TaskSuccess, $"Task {suffix}".Trim() + $" completed with success in {sw.Elapsed.ToFormattedDuration()}.");
                            }
                            catch (Exception ex)
                            {
                                engineLogger.LogError(TaskError, ex, nameof(this.Execute));
                                logger.LogError(TaskError, ex, nameof(this.Execute));
                                return; //ignore continuation
                            }
                            try
                            {
                                HandleContinuations();
                            }
                            catch (Exception ex)
                            {
                                engineLogger.LogError(TaskError, ex, nameof(this.HandleContinuations));
                                logger.LogError(TaskError, ex, nameof(this.HandleContinuations));
                            }

                        } //using LoggingProvider
                    } //using loggerfactory
                }; //using Scope
            };
            logger.LogTrace(302, $"WorkItemForQueue created: {this.GetType().Name}");
            return result;
        }
    }
}
