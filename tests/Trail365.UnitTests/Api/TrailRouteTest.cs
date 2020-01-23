using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Seeds;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "Routing")]
    [Trait("Category", "BuildVerification")]
    public class TrailRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TrailRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldCreateTrailsViaEntity()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                var seed = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = $"Demo Track",
                    Description = $"Description for Demo Track",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.User
                };
                var json = JsonConvert.SerializeObject(seed);
                server.PostToServer($"{RouteName.TrailsApi}/raw", json).EnsureSuccessStatusCode();
                Assert.Single(server.TrailContext.Trails);
            }
        }

        [Fact]
        public void ShouldCreateTrailsViaDto_WithoutImages()
        {
            var testData = TrailDtoProvider.CreateInstance();

            using (var host = TestHostBuilder.Empty(this.OutputHelper).WithTrailContext().UseStaticAuthenticationAsAdmin().Build())
            {
                Assert.Empty(host.TrailContext.Blobs.ToList());
                foreach (var seed in testData.All)
                {
                    using (var client = host.CreateClient())
                    {
                        client.PostTrailViaApi(seed).EnsureSuccessStatusCode();
                    }
                }
                Assert.Equal(testData.All.Length, host.TrailContext.Trails.ToList().Count);
                //host.WaitForBackgroundTasks();
                //Assert.Equal(testData.All.Length * (2 + 4), host.TrailContext.Blobs.ToList().Count);
            }
        }

        [Fact]
        public void ShouldGetTrailsViaDto()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                using (var client = server.CreateClient())
                {
                    var responseMessage = client.GetTrailsViaApi().EnsureSuccessStatusCode();
                }
            }
        }

        [Fact]
        public void ShouldCreateTrailsFromMultiFileGpx()
        {
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                using (var client = server.CreateClient())
                {
                    client.PostMultiTrailGpxViaApi(GpxTracks.MultiFile3Sample, "demo").EnsureSuccessStatusCode();
                }
            }
        }

        [Fact]
        public void ShouldCreateTrailsViaDto()
        {
            var testData = TrailDtoProvider.CreateInstance();

            using (var host = TestHostBuilder.Empty(this.OutputHelper).UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                foreach (var seed in testData.All)
                {
                    using (var client = host.CreateClient())
                    {
                        client.PostTrailViaApi(seed).EnsureSuccessStatusCode();
                    }
                }

                Assert.Equal(testData.All.Length, host.TrailContext.Trails.ToList().Count);

                var response = host.GetFromServer(RouteName.TrailsApi).EnsureSuccessStatusCode();
                string resultJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var trails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrailDto>>(resultJson);
                Assert.Equal(trails.Count, testData.All.Length);

                int memberTrails = trails.Where(t => t.GpxDownloadAccess == AccessLevel.Member).Count();
                Assert.Equal(memberTrails, testData.MemberTrailsCounter);

                int adminTrails = trails.Where(t => t.GpxDownloadAccess == AccessLevel.Administrator).Count();
                Assert.Equal(adminTrails, testData.AdministratorTrailsCounter);

                int publicTrails = trails.Where(t => t.GpxDownloadAccess == AccessLevel.Public).Count();
                Assert.Equal(publicTrails, testData.PublicTrailsCounter);

                int userTrails = trails.Where(t => t.GpxDownloadAccess == AccessLevel.User).Count();
                Assert.Equal(userTrails, testData.UserTrailsCounter);

                int moderatorTrails = trails.Where(t => t.GpxDownloadAccess == AccessLevel.Moderator).Count();
                Assert.Equal(moderatorTrails, testData.ModeratorTrailsCounter);
            }
        }
    }
}
