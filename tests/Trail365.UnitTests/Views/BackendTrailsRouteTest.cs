using System;
using System.Linq;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    /// <summary>
    /// Route-Test means: Razor Views on this Route are rendered dureing the test!
    /// </summary>
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class BackendTrailsRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public BackendTrailsRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void Index()
        {
            using (var server = TestHostBuilder.DefaultForBackend(this.OutputHelper).Build())
            {
                server.GetFromServer("Backend/Trails/Index").EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public void Edit()
        {
            var testData = TrailDtoProvider.CreateInstance();
            using (var server = TestHostBuilder.DefaultForBackend().Build())
            {
                var hc = server.CreateTrailsController();
                foreach (var trail in testData.All)
                {
                    hc.PostTrail(trail);
                }
                Assert.Equal(testData.All.Length, server.TrailContext.Trails.Count());
                foreach (var seed in testData.All)
                {
                    var response = server.GetFromServer($"Backend/Trails/Edit/{seed.ID}");
                    //var response = server.GetFromServer($"Backend/Trails/Edit");
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
