using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class StoryEditorControllerTest
    {
        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForBackend().WithTrailContext().WithIdentityContext().Build())
            {
                using (var hc = host.CreateStoryEditorController())
                {
                    Assert.NotNull(hc);
                    Assert.NotNull(hc.Url);
                }
            }
        }
    }
}
