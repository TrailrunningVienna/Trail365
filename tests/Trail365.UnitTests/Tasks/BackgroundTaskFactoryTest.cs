using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trail365.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Tasks
{
    [Trait("Category", "BuildVerification")]
    public class BackgroundTaskFactoryTest
    {
        public class ExceptionTask : BackgroundTask
        {
            protected override Task Execute(CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("DEMO");
            }
        }

        public class LogTask : BackgroundTask
        {
            protected override Task Execute(CancellationToken cancellationToken)
            {
                this.Context.DefaultLogger.LogTrace("TraceMessage");
                this.Context.DefaultLogger.LogInformation("InformationMessage");
                return Task.CompletedTask;
            }
        }

        private readonly ITestOutputHelper OutputHelper;

        public BackgroundTaskFactoryTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldCreateExceptionTask()
        {
            using (var host = TestHostBuilder.Empty().WithTaskContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var queue = new BackgroundTaskQueue();
                Assert.NotNull(host.RootUrl);
                var task = BackgroundTaskFactory.CreateTask<ExceptionTask>(host.ServiceScopeFactory, host.RootUrl);
                task.Queue(queue);
                var t = queue.DequeueAsync(CancellationToken.None).GetAwaiter().GetResult();
                t(CancellationToken.None).GetAwaiter().GetResult();
                var errorItem = host.TaskContext.TaskLogItems.Where(l => l.Level == "Error").Single();
                Assert.Contains("DEMO", errorItem.LogMessage);
            }
        }

        [Fact]
        public void ShouldCreateLogTask()
        {
            using (var host = TestHostBuilder.Empty().WithTaskContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var queue = new BackgroundTaskQueue();
                Assert.NotNull(host.RootUrl);
                var task = BackgroundTaskFactory.CreateTask<LogTask>(host.ServiceScopeFactory, host.RootUrl);
                task.Queue(queue);
                var t = queue.DequeueAsync(CancellationToken.None).GetAwaiter().GetResult();
                t(CancellationToken.None).GetAwaiter().GetResult();
                Assert.NotEmpty(host.TaskContext.TaskLogItems);
            }
        }
    }
}
