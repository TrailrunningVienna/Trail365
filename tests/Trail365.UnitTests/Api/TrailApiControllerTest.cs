using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "BuildVerification")]
    public class TrailApiControllerTest
    {
        //private readonly bool SkipEnabled = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private readonly ITestOutputHelper OutputHelper;

        public TrailApiControllerTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldCreateTrailWithoutScrapingService()
        {
            using (var host = TestHostBuilder.Empty().UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                var hc = host.CreateTrailsController();

                var seed = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = $"Demo Track",
                    Description = $"Description for Demo Track",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.User
                };
                hc.PostRawTrail(seed).GetAwaiter().GetResult();
            }
        }

        [Fact]
        public void ShouldCreateListAndDeleteTrailsFromMultiFileGpx()
        {
            //Skip.If(SkipEnabled);
            using (var host = TestHostBuilder.Empty().UseStaticAuthenticationAsAdmin().WithTrailContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var hc = host.CreateTrailsController();

                using (Stream sample = File.OpenRead(GpxTracks.MultiFile3Sample))
                {
                    IFormFile file = new FormFile(sample, 0, sample.Length, "myfile.gpx", "myfile.gpx");
                    hc.CreateTracksPost(file, "basecampsimu").GetAwaiter().GetResult();
                }
                Assert.Equal(3, host.TrailContext.Trails.Count());
                Assert.Equal(3, host.TrailContext.Blobs.Count()); //no scraping blobs but gpx blob!
            }
        }

        [Theory]
        //[InlineData(true)] //scraping==true not testable inside UnitTests/TestHost => needs more investiagtions!
        [InlineData(false)]
        public void ShouldCreateListAndDeleteTrails(bool scraping)
        {
            var testData = TrailDtoProvider.CreateInstance();
            var hostBuilder = TestHostBuilder.Empty(this.OutputHelper).UseStaticAuthenticationAsAdmin().WithTrailContext();
            if (scraping)
            {
                hostBuilder = hostBuilder.UseBackgroundTaskEngine();
            }

            using (var host = hostBuilder.Build())
            {
                var hc = host.CreateTrailsController();

                if (scraping)
                {
                    Assert.Empty(host.TaskContext.TaskLogItems);
                }

                foreach (var trail in testData.All)
                {
                    hc.PostTrail(trail);
                }

                if (scraping)
                {
                    host.WaitForBackgroundTasks(30);
                    var items = host.TaskContext.TaskLogItems.ToList();
                }

                Assert.Equal(testData.All.Length, host.TrailContext.Trails.ToList().Count);

                if (scraping)
                {
                    Assert.Equal(testData.All.Length * (4 + 4 + 1), host.TrailContext.Blobs.ToList().Count);
                }
                else
                {
                    Assert.Equal(testData.All.Length * (1), host.TrailContext.Blobs.ToList().Count);
                }

                //Test ListAccess settings after Upload
                Assert.Equal(host.TrailContext.Trails.Where(t => t.ListAccess == AccessLevel.Member).Count(), testData.MemberTrailsCounter);
                Assert.Equal(host.TrailContext.Trails.Where(t => t.ListAccess == AccessLevel.Public).Count(), testData.PublicTrailsCounter);
                Assert.Equal(host.TrailContext.Trails.Where(t => t.ListAccess == AccessLevel.Administrator).Count(), testData.AdministratorTrailsCounter);
                Assert.Equal(host.TrailContext.Trails.Where(t => t.ListAccess == AccessLevel.User).Count(), testData.UserTrailsCounter);
                Assert.Equal(host.TrailContext.Trails.Where(t => t.ListAccess == AccessLevel.Moderator).Count(), testData.ModeratorTrailsCounter);
                var results = hc.GetTrails().GetAwaiter().GetResult().Value.ToArray();
                foreach (var trail in results)
                {
                    Assert.True(trail.DistanceMeters.HasValue, trail.Name);
                    Assert.True(trail.AscentMeters.HasValue, trail.Name);
                    Assert.True(trail.DescentMeters.HasValue, trail.Name);
                    Assert.True(trail.MinimumAltitude.HasValue, trail.Name);
                    Assert.True(trail.MaximumAltitude.HasValue, trail.Name);
                    hc.DeleteTrail(trail.ID).GetAwaiter().GetResult();
                }
                Assert.Empty(host.TrailContext.Trails.ToList());
                Assert.Empty(host.TrailContext.Blobs.ToList());
            }
        }

        /// <summary>
        /// starts with empty database, uses FileSystemBlobStorage
        /// </summary>
        [Fact]
        public void ShouldCreateWorkingTrail()
        {
            using (var host = TestHostBuilder.Empty().UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                var testTrails = TrailDtoProvider.CreateInstance();
                Assert.Empty(host.TrailContext.Trails);
                Assert.Empty(host.TrailContext.Blobs);
                var hc = host.CreateTrailsController();
                foreach (var trailDto in testTrails.All)
                {
                    var resultingTrailDto = hc.PostTrail(trailDto);
                    Assert.NotNull((resultingTrailDto));
                }
                Assert.Equal(testTrails.All.Length, host.TrailContext.Trails.ToList().Count);
                Assert.Equal(testTrails.All.Length * (1), host.TrailContext.Blobs.ToList().Count);
            }
        }
    }
}
