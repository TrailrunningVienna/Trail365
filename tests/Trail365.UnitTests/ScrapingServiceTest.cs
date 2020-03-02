using System.Linq;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ScrapingServiceTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public ScrapingServiceTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldScrape()
        {
            var testTrails = TrailViewModelProvider.CreateInstance();
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(this.OutputHelper).WithTrailContext().Build())
            {
                Assert.True(host.BlobService.IsNull);
                Assert.True(host.Scraper.IsNull);

                host.InitWithViewModel(testTrails.All);
                Assert.Equal(testTrails.All.Length, host.TrailContext.Trails.ToList().Count);
                var trailToTest = host.TrailContext.Trails.First();
                Assert.False(trailToTest.ScrapedUtc.HasValue);
                Assert.False(trailToTest.PreviewImageID.HasValue);
                Assert.False(trailToTest.SmallPreviewImageID.HasValue);
                Assert.False(trailToTest.MediumPreviewImageID.HasValue);

                host.ScrapingService.ScrapeTrail(host.TrailContext, trailToTest, host.RootUrl);
                host.TrailContext.SaveChanges();

                Assert.Equal(3 + 4, host.TrailContext.Blobs.ToList().Count);
                Assert.True(trailToTest.ScrapedUtc.HasValue);
                Assert.True(trailToTest.PreviewImageID.HasValue);
                Assert.True(trailToTest.SmallPreviewImageID.HasValue);
                Assert.True(trailToTest.MediumPreviewImageID.HasValue);
            }
        }
    }
}
