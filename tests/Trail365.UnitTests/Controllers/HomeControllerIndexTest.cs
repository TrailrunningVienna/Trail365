using Trail365.Configuration;
using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class HomeControllerIndexTest
    {
        [Fact]
        public void ShouldRedirectToTrails()
        {
            Features features = new Features()
            {
                Trails = true,
                Events = false,
                Stories = false,
            };

            using (var host = TestHostBuilder.DefaultForFrontendAsUser(features).Build())
            {
                var hc = host.CreateHomeController();
                Microsoft.AspNetCore.Mvc.RedirectToActionResult res = hc.Index(null) as Microsoft.AspNetCore.Mvc.RedirectToActionResult;
                Assert.NotNull(res);
                Assert.Equal("TrailNews", res.ControllerName);
                Assert.Equal("Index", res.ActionName);
            }
        }
    }
}
