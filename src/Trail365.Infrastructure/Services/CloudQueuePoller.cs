using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace Trail365.Services
{
    public class CloudQueuePoller
    {

        //https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific#azure-storage
        //https://raw.githubusercontent.com/Azure/azure-storage-net/master/Lib/Common/RetryPolicies/ExponentialRetry.cs
        //lessons learned: the entire "RetryPolicy feature on the API is for HTTP-Requests but NOT for long polling (REST-API returns http.ok for empty queue!!

        private const int deltaPercent = 120;
        private const int delayMillisecondsMinValue = 50;
        private const int delayMillisecondsMaxValue = 1000 * 8; //8 seconds

        public CloudQueuePoller(CloudQueue queue, TimeSpan visibilityTimeout)
        {
            _visibilityTimeout = visibilityTimeout;
            TaskQueue = queue;
        }

        private readonly CloudQueue TaskQueue;
        private readonly TimeSpan _visibilityTimeout;
        private readonly QueueRequestOptions qro = new QueueRequestOptions();
        private readonly OperationContext cntx = new OperationContext();

        /// <summary>
        /// Task.Result cannot be NULL. in case of any timeout/canceling, a exception (cancellation, timout etc must be thrown!
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CloudQueueMessage> WaitForQueueMessage(CancellationToken cancellationToken)
        {
            int nextDelayInMilliseconds = delayMillisecondsMinValue;
            while (true)
            {
                var workItem = await this.TaskQueue.GetMessageAsync(_visibilityTimeout, qro, cntx, cancellationToken);
                if (workItem != null) return workItem;

                if (nextDelayInMilliseconds < delayMillisecondsMaxValue)
                {
                    nextDelayInMilliseconds = nextDelayInMilliseconds * deltaPercent / 100;
                }

                await Task.Delay(nextDelayInMilliseconds);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
