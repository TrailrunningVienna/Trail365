using System;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class WarmupRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public WarmupRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void Warmup()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsNotLoggedIn().WithIdentityContext().WithTaskContext().WithTrailContext().Build())
            {
                server.GetFromServer("Warmup").EnsureSuccessStatusCode();
            }
        }
    }
}
