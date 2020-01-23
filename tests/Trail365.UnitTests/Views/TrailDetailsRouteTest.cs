using System;
using Trail365.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    /// <summary>
    /// Route-Test means: Razor Views on this Route are rendered dureing the test!
    /// </summary>
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class TrailDetailsRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TrailDetailsRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldRenderSingleTrailWithDetails()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser(this.OutputHelper).Build())
            {
                var seed = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = $"Demo Track",
                    Description = $"# Description for Demo Track{Environment.NewLine}## line2",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.Public
                };
                Assert.False(seed.PreviewImageID.HasValue);
                Assert.False(seed.GpxBlobID.HasValue);
                host.TrailContext.Trails.Add(seed);
                host.TrailContext.SaveChanges();

                string[] urlsToVerify = new string[]
                {
                    $"{RouteName.TrailDetails}?ID={seed.ID}",
                    $"{RouteName.TrailDetails}/{seed.ID}",
                    host.RootUrl.GetTrailDetailsUrl(seed.ID,true,false)
                };

                foreach (string url in urlsToVerify)
                {
                    var response = host.GetFromServer(url);
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
