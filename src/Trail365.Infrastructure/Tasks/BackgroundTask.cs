using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Trail365.Data;

namespace Trail365.Tasks
{
    public abstract class BackgroundTask
    {
        public string Caption { get; set; }

        internal ILogger logger = NullLogger.Instance;

        public BackgroundTaskContext Context { get; set; }

        protected virtual void OnBeforeExecute()
        { }

        protected abstract Task Execute(CancellationToken cancellationToken);

        public void Queue(IBackgroundTaskQueue queue, bool disabledLogging = false)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            if (this.Context == null)
            {
                throw new InvalidOperationException("Context must be set");
            }
            queue.QueueBackgroundWorkItem(this.CreateWorkItemForQueue(this.Context, disabledLogging));
        }

        private static readonly EventId TaskStarted = new EventId(300, nameof(TaskStarted));
        private static readonly EventId TaskSuccess = new EventId(301, nameof(TaskSuccess));
        private static readonly EventId TaskError = new EventId(301, nameof(TaskError));

        private Func<CancellationToken, Task> CreateWorkItemForQueue(BackgroundTaskContext context, bool logDisabled)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (logger == null) throw new InvalidOperationException(nameof(logger));
            Func<CancellationToken, Task> result = async (cts) =>
            {
                using (var scope = context.ServiceScopeFactory.CreateScope())
                {
                    context.ServiceProvider = scope.ServiceProvider;

                    using (var loggerFactory = new LoggerFactory())
                    {
                        context.LoggerFactory = loggerFactory;
                        var dbContext = context.ServiceProvider.GetRequiredService<TaskContext>();
                        using (ILoggerProvider loggerProvider = new BackgroundTaskLoggingProvider(dbContext, logDisabled))
                        {
                            loggerFactory.AddProvider(loggerProvider);

                            var engineLogger = loggerFactory.CreateLogger(nameof(BackgroundTask)); //logging to TaskLog but as a Engine Category, not as a Task-Implementation Category!

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

                            string currentTaskName = this.GetType().Name;
                            context.DefaultLogger = context.LoggerFactory.CreateLogger(currentTaskName);
                            string suffix = string.Empty;

                            if (!string.IsNullOrEmpty(this.Caption))
                            {
                                suffix = $" {this.Caption}";
                            }

                            try
                            {
                                engineLogger.LogInformation(TaskStarted, $"{currentTaskName}{suffix} started.");
                                var sw = Stopwatch.StartNew();
                                await this.Execute(cts);
                                sw.Stop();
                                engineLogger.LogInformation(TaskSuccess, $"{currentTaskName}{suffix} completed with success in {sw.ElapsedMilliseconds} ms.");
                            }
                            catch (Exception ex)
                            {
                                engineLogger.LogError(TaskError, ex, nameof(this.Execute));
                                logger.LogError(TaskError, ex, nameof(this.Execute));
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
