using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class StoryControllerTest
    {
        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().Build())
            {
                using (var hc = host.CreateStoryController())
                {
                    Assert.NotNull(hc);
                    Assert.NotNull(hc.Url);
                }
            }
        }
    }
}
