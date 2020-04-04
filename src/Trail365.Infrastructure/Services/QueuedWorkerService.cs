using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Internal;
using System.Diagnostics;
using Trail365.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Trail365.Services
{

    public class QueuedWorkerService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CloudQueue TaskQueue { get; }

        public bool IsRunning { get; private set; } = false;

        public static bool TryCreateStorageQueue(string connectionString, string queueName, out CloudQueue queue)
        {
            queue = null;
            if (string.IsNullOrEmpty(connectionString)) return false;
            if (CloudStorageAccount.TryParse(connectionString, out var cloudStorageAccount) == false)
            {
                return false;
            }
            CloudQueueClient queueClient = cloudStorageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(queueName);
            return (queue != null);
        }

        private QueuedWorkerService(IServiceScopeFactory serviceScopeFactory, AppSettings settings,  ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.CloudStorageEnabled == false) throw new InvalidOperationException("CloudService not enabled, AzureBlobService should not be added to DI");
            string expandedConnectionString = settings.ConnectionStrings.GetResolvedCloudStorageConnectionString();

            if (TryCreateStorageQueue(expandedConnectionString, "taskrequest", out var cl))
            {
                this.TaskQueue = cl;
                this.TaskQueue.CreateIfNotExists();
            }
            else
            {
                throw new InvalidOperationException("CloudStorageConnectionstring not valid/empty");
            }
        }

        public QueuedWorkerService(IServiceScopeFactory serviceScopeFactory, IOptionsMonitor<AppSettings> settings, ILogger<QueuedWorkerService> logger): this(serviceScopeFactory, settings.CurrentValue, logger)
        {
        }



        public void TrailAnalyzer()
        {
            //var task = BackgroundTaskFactory.CreateTask<TrailAnalyzerTask>(this._serviceScopeFactory,null);// this._serviceScopeFactory, this.Url);
            //task.TrailID = Guid.NewGuid();

            //task.StartNow(CancellationToken.None).Wait();
        }

        protected async Task ExecuteWorkItem(CloudQueueMessage message, CancellationToken cancellationToken)
        {
            //1. do the work (if it takes longer, can we update the visibility time ?)
            //2. write response message
            //3. delete the original message => handles outside
            await Task.Delay(1000);
        }



        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Worker Service is starting.");
            long taskCounter = 0;
            long errorCounter = 0;
            this.IsRunning = true;
            CloudQueuePoller poller = new CloudQueuePoller(this.TaskQueue, TimeSpan.FromMinutes(10));

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var workItem = await poller.WaitForQueueMessage(cancellationToken);
                    Guard.AssertNotNull(workItem);
                    taskCounter += 1;
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        await ExecuteWorkItem(workItem, cancellationToken);
                        sw.Stop();
                        _logger.LogDebug($"Workitem execution completed in {sw.ElapsedMilliseconds}ms");
                        await this.TaskQueue.DeleteMessageAsync(workItem, cancellationToken);
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
            _logger.LogInformation($"Queued Worker Service is stopping ({taskCounter} tasks executed, {errorCounter} faulted).");
        }
    }
}
