using System;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    /// <summary>
    /// Route-Test means: Razor Views on this Route are rendered dureing the test!
    /// </summary>
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class HomeRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public HomeRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldRenderEmptyIndexForAdmin()
        {
            using (var server = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var response = server.GetFromServer("Home/Index");
                response.EnsureRedirectStatusCode();
            }
        }

        [Fact]
        public void ShouldRenderEmptyIndexForNotLoggedInUser()
        {
            //Without AuthenticationInfo,
            using (var server = TestHostBuilder.Empty().UseFileSystemStorage().UseTestOutputHelper(OutputHelper).WithTrailContext().UseStaticAuthenticationAsNotLoggedIn().Build())
            {
                var response = server.GetFromServer(RouteName.HomeIndex);
                response.EnsureRedirectStatusCode();
            }
        }

        [Fact]
        public void Layout()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsNotLoggedIn().WithTrailContext().Build())
            {
                var response = server.GetFromServer("Home/Layout");
                response.EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public void Layout_Default()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsNotLoggedIn().WithTrailContext().Build())
            {
                var response = server.GetFromServer("Home/Layout/Default");
                response.EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public void Index_AsAdmin()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsAdmin().WithTrailContext().UseFileSystemStorage().Build())
            {
                var response = server.GetFromServer(RouteName.HomeIndex);
                response.EnsureRedirectStatusCode();
            }
        }
    }
}
