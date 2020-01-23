using System;
using Trail365.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    /// <summary>
    /// Route-Test means: Razor Views on this Route are rendered during the test!
    /// </summary>
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class TrailPreviewRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TrailPreviewRouteTest(ITestOutputHelper helper)
        {
            OutputHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        [Fact]
        public void ShouldCalculatePreviewForSingleTrail()
        {
            using (var host = TestHostBuilder.DefaultForBackend(this.OutputHelper).Build())
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
                    $"{RouteName.TrailPreview}?ID={seed.ID}"
                };

                foreach (string url in urlsToVerify)
                {
                    var response = host.GetFromServer(url);
                    var redirectTo = response.EnsureRedirectStatusCode();
                    Assert.Contains(RouteName.TrailDetails, redirectTo, StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
    }
}
