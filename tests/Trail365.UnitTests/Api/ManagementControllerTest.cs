using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "BuildVerification")]
    public class ManagementControllerTest
    {
        [Fact]
        public void ShouldCreateControllerAndBackup()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().WithTaskContext().WithBackupDirectory(System.IO.Path.GetTempPath()).Build())
            {
                var hc = host.CreateManagementController();
                Assert.NotNull(hc);
                hc.Backup(false);
                Assert.Empty(host.TaskContext.TaskLogItems); //no logging to prevent endless sync!
            }
        }
    }
}
