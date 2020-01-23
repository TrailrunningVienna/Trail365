using System;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    /// <summary>
    /// Route-Test means: Razor Views on this Route are rendered dureing the test!
    /// </summary>
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class BackendHomeRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public BackendHomeRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void Index()
        {
            using (var server = TestHostBuilder.DefaultForBackend().Build())
            {
                var response = server.GetFromServer("Backend/Home/Index");
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
