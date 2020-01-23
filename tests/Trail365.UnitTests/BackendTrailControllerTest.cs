using System;
using System.Linq;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class BackendTrailControllerTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public BackendTrailControllerTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForBackend(this.OutputHelper).Build())
            {
                var hc = host.CreateBackendTrailsController();
            }
        }

        [Fact]
        public void Edit()
        {
            var testData = TrailDtoProvider.CreateInstance();
            using (var server = TestHostBuilder.DefaultForBackend(this.OutputHelper).Build())
            {
                var seedingController = server.CreateTrailsController();
                foreach (var trail in testData.All)
                {
                    OutputHelper.WriteLine("Start: " + trail.Name);
                    seedingController.PostTrail(trail);
                    OutputHelper.WriteLine("END: " + trail.Name);
                }
                Assert.Equal(testData.All.Length, server.TrailContext.Trails.Count());
                var trailController = server.CreateBackendTrailsController();
                foreach (var seed in testData.All)
                {
                    var model = trailController.Edit(seed.ID).ToModel<TrailBackendViewModel>();
                    Assert.Equal(seed.ID, model.ID);
                    Assert.Equal(seed.Name, model.Name);
                    Assert.Equal(seed.Description, model.Description);
                }
            }
        }

        [Fact]
        public void Delete()
        {
            var testData = TrailDtoProvider.CreateInstance();
            using (var server = TestHostBuilder.DefaultForBackend(this.OutputHelper).Build())
            {
                var seedingController = server.CreateTrailsController();
                foreach (var trail in testData.All)
                {
                    OutputHelper.WriteLine("Start: " + trail.Name);
                    seedingController.PostTrail(trail);
                    OutputHelper.WriteLine("END: " + trail.Name);
                }
                Assert.Equal(testData.All.Length, server.TrailContext.Trails.Count());
                var trailController = server.CreateBackendTrailsController();

                foreach (var seed in testData.All)
                {
                    var model = trailController.Delete(seed.ID).ToModel<TrailBackendViewModel>().GetAwaiter().GetResult();

                    Assert.Equal(seed.ID, model.ID);
                    Assert.Equal(seed.Name, model.Name);
                    Assert.Equal(seed.Description, model.Description);
                    trailController.DeleteConfirmed(seed.ID);
                }
            }
        }
    }
}
