using System;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class HealthRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public HealthRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Theory]
        [InlineData("Production")]
        [InlineData("Development")]
        public void Health(string environment)
        {
            using (var host = TestHostBuilder.Empty(this.OutputHelper).UseEnvironment(environment).UseStaticAuthenticationAsNotLoggedIn().Build())
            {
                host.GetFromServer("health").EnsureSuccessStatusCode();
            }
        }
    }
}
