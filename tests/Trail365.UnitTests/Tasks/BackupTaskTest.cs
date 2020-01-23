using System.Threading;
using Trail365.Tasks;
using Trail365.Web.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Tasks
{
    [Trait("Category", "BuildVerification")]
    public class BackupTaskTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public BackupTaskTest(ITestOutputHelper outputHelper)
        {
            this.OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldExecuteBackupTask()
        {
            var builder = TestHostBuilder.DefaultForBackend(this.OutputHelper).WithIdentityContext().WithTaskContext().WithTrailContext().WithBackupDirectory();
            var backupDir = builder.GetBackupDirectoryDefault();
            using (var host = builder.Build())
            {
                var queue = new BackgroundTaskQueue();
                Assert.NotNull(host.RootUrl);
                var task = BackgroundTaskFactory.CreateTask<BackupTask>(host.ServiceScopeFactory, host.RootUrl);
                task.Queue(queue);
                var t = queue.DequeueAsync(CancellationToken.None).GetAwaiter().GetResult();
                t(CancellationToken.None).GetAwaiter().GetResult();
            }
        }
    }
}
