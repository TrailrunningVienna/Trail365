using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Trail365.Services
{
    public sealed class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;

        public IBackgroundTaskQueue TaskQueue { get; }

        public bool IsRunning { get; private set; } = false;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger)
        {
            this.TaskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");
            long taskCounter = 0;
            long errorCounter = 0;
            this.IsRunning = true;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var workItem = await this.TaskQueue.DequeueAsync(cancellationToken);
                    taskCounter += 1;
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        await workItem(cancellationToken);
                        sw.Stop();
                        _logger.LogDebug($"Workitem execution completed in {sw.ElapsedMilliseconds}ms");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{ex.GetType().Name} occurred executing workItem: {ex.Message}.", nameof(workItem));
                        errorCounter += 1;
                    }
                }
            }
            finally
            {
                this.IsRunning = false;
            }
            _logger.LogInformation($"Queued Hosted Service is stopping ({taskCounter} tasks executed, {errorCounter} faulted).");
        }
    }
}
