using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Trail365.Tasks
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public bool IsEmpty => _workItems.IsEmpty;

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken); //waits endless or cancellation for "_signal.Release"
            var result = _workItems.TryDequeue(out var workItem);
            if (!result)
            {
                return null;
            }
            return workItem;
        }
    }
}
