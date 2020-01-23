using System;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    /// <summary>
    /// Route-Test means: Razor Views on this Route are rendered dureing the test!
    /// </summary>
    [Trait("Category", "Routing")]
    [Trait("Category", "BuildVerification")]
    public class FrontendRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public FrontendRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void Profile()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser(this.OutputHelper).Build())
            {
                host.CreateFrontendController(); //THIS calls "EnsureUser"!
                var response = host.GetFromServer(RouteName.FrontendProfile);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
