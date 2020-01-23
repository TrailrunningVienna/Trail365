using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trail365
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);

        bool IsEmpty { get; }
    }
}
