using System;
using System.IO;
using System.Linq;
using Trail365.Seeds;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrailImporterServiceTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TrailImporterServiceTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldImportTrail()
        {
            using (var host = TestHostBuilder.Empty(this.OutputHelper).WithTrailContext().Build())
            {
                Assert.NotNull(host.TrailImporterService);
                string xml = File.ReadAllText(GpxTracks.MultiFile3Sample);
                var result = host.TrailImporterService.Execute(host.TrailContext, xml, "bc", DateTime.UtcNow, host.RootUrl);
                Assert.Equal(3, host.TrailContext.Trails.Count());
                Assert.Equal(3, result.InsertedTrails.Count);
                Assert.Empty(result.UpdatedTrails);
                Assert.Empty(result.UpdatedBlobs);
                Assert.Equal(3, result.InsertedBlobs.Count);

                var result2 = host.TrailImporterService.Execute(host.TrailContext, xml, "bc", DateTime.UtcNow, host.RootUrl);
                Assert.Equal(3, host.TrailContext.Trails.Count());
                Assert.Empty(result2.InsertedTrails);
                Assert.Empty(result2.UpdatedTrails);
                Assert.Empty(result2.InsertedBlobs);
                Assert.Empty(result.UpdatedBlobs);

                host.TrailImporterService.TrackPreprocessor = (s) => s + " "; //add space to the xml => should be detected as change!

                var result3 = host.TrailImporterService.Execute(host.TrailContext, xml, "bc", DateTime.UtcNow, host.RootUrl);
                Assert.Equal(3, host.TrailContext.Trails.Count());
                Assert.Empty(result3.InsertedTrails);
                Assert.Empty(result3.UpdatedTrails);
                Assert.Equal(3, result3.UpdatedBlobs.Count);
                Assert.Empty(result3.InsertedBlobs);
            }
        }
    }
}
