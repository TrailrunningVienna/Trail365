using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class TrailPreviewControllerTest
    {
        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForBackend().Build())
            {
                using (var hc = host.CreateTrailPreviewController())
                {
                    Assert.NotNull(hc);
                    Assert.NotNull(hc.Url);
                }
            }
        }
    }
}
