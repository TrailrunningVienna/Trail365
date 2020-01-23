using System;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class ManagementRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public ManagementRouteTest(ITestOutputHelper helper)
        {
            OutputHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        [Fact]
        public void Backup()
        {
            using (var host = TestHostBuilder.Empty(this.OutputHelper).UseStaticAuthenticationAsNotLoggedIn().WithTrailContext().WithBackupDirectory().Build())
            {
                host.GetFromServer(RouteName.AppBackup).EnsureSuccessStatusCode();
            }
        }
    }
}
