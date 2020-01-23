using System.Threading;
using System.Threading.Tasks;
using Trail365.Tasks;
using Xunit;

namespace Trail365.UnitTests.Tasks
{
    [Trait("Category", "BuildVerification")]
    public class BackgroundTaskQueueTest
    {
        private readonly BackgroundTaskQueue TestQueue = new BackgroundTaskQueue();

        private static Task workItemImpl(CancellationToken cts)
        {
            return Task.CompletedTask;
        }

        [Fact]
        public void ShouldHandleEmpty()
        {
            var queue = (IBackgroundTaskQueue)this.TestQueue;
            Assert.True(queue.IsEmpty);
            queue.QueueBackgroundWorkItem(workItemImpl);
            Assert.False(queue.IsEmpty);
            var t = queue.DequeueAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.NotNull(t);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void ShouldHandleEmpty_2()
        {
            var queue = (IBackgroundTaskQueue)this.TestQueue;
            Assert.True(queue.IsEmpty);
            queue.QueueBackgroundWorkItem(workItemImpl);
            queue.QueueBackgroundWorkItem(workItemImpl);
            Assert.False(queue.IsEmpty);
            var t = queue.DequeueAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.NotNull(t);
            Assert.False(queue.IsEmpty);
            var t1 = queue.DequeueAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.NotNull(t1);
            Assert.True(queue.IsEmpty);
        }
    }
}
