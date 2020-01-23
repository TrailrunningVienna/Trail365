using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class AuthControllerTest
    {
        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().Build())
            {
                var hc = host.CreateAuthController();
                Assert.NotNull(hc);
            }
        }
    }
}
;
