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

        //[SkippableTheory]
        //[InlineData(640)]
        //[InlineData(320)]
        //[InlineData(160)]
        //[InlineData(1024)]
        //public void ShouldScrapeUsingBrowserless(int squareLength)
        //{
        //    Skip.If(true);
        //    PuppeteerScraper scraper = PuppeteerScraper.Create();
        //    //scraper.FullPage = true;
        //    var uri = new System.UriBuilder("https://trailexplorer-qa.azurewebsites.net/Index?mode=snapshot&style=outdoor&height=920&width=920&debug=true&gpxsource=https%3A%2F%2Fmssdev.blob.core.windows.net%2Fpublicblob%2Fgpx%2F440bcd3601374035bd426b3221df4291.gpx").Uri;

        //    var resultAsPng = scraper.ScreenshotAsync(uri, new System.Drawing.Size(squareLength, squareLength)).GetAwaiter().GetResult();
        //    System.IO.File.WriteAllBytes($"c:\\work\\previewBrowserless{squareLength}.png", resultAsPng);
        //}

        //[SkippableTheory]
        //[InlineData(640)]
        //[InlineData(320)]
        //[InlineData(160)]
        //[InlineData(1024)]
        //public void ShouldScrapeUsingLocalChromium(int squareLength)
        //{
        //    Skip.If(true);
        //    MapScraper scraper = PuppeteerScraper.Create();
        //    var uri = new System.UriBuilder("https://trailexplorer-qa.azurewebsites.net/Index?mode=snapshot&style=outdoor&height=920&width=920&debug=true&gpxsource=https%3A%2F%2Fmssdev.blob.core.windows.net%2Fpublicblob%2Fgpx%2F440bcd3601374035bd426b3221df4291.gpx").Uri;

        //    var resultAsPng = scraper.ScreenshotAsync(uri, new System.Drawing.Size(squareLength, squareLength)).GetAwaiter().GetResult();
        //    System.IO.File.WriteAllBytes($"c:\\work\\previewLocal{squareLength}.png", resultAsPng);
        //}
    }
}
