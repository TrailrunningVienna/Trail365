using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class FrontendControllerTest
    {
        [Fact]
        public void ShouldGetProfileController()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                using (var hc = host.CreateFrontendController())
                {
                    Assert.NotNull(hc.User); //required for this test!
                    var vm = hc.Profile().ToModel<ProfileSettingsViewModel>();
                    Assert.NotNull(vm);
                }
            }
        }

        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                using (var hc = host.CreateFrontendController())
                {
                    Assert.NotNull(hc);
                    Assert.NotNull(hc.Url);
                }
            }
        }
    }
}
