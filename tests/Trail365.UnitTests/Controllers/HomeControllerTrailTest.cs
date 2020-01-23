using System;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class HomeControllerTrailTest
    {
        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser().Build())
            {
                var hc = host.CreateHomeController();
            }
        }

        /// <summary>
        /// Bug & Fix
        /// </summary>
        [Fact]
        public void ShouldGetIndexForNullObjects()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser().Build())
            {
                //to reproduce the exception we need some OTHER Trail WITH associated Images!
                var imageSeed = new Blob
                {
                    FolderName = "png",
                    Url = "http://www.image.com"
                };

                host.TrailContext.Blobs.Add(imageSeed);

                var seed0 = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = $"Demo Track WITH Image",
                    Description = $"Description for Demo Track WITH Image",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.Public,
                    PreviewImageID = imageSeed.ID,
                };

                var seed1 = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = $"Demo Track",
                    Description = $"Description for Demo Track",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.Public
                };

                Assert.False(seed1.PreviewImageID.HasValue);
                Assert.False(seed1.GpxBlobID.HasValue);
                host.TrailContext.Trails.Add(seed1);
                host.TrailContext.Trails.Add(seed0);

                Assert.True(host.TrailContext.SaveChanges() > 2);
                var hc = host.CreateHomeController();
                var indexResult = hc.AllTrails(null).ToModel<HomeViewModel>();
                Assert.NotNull((indexResult));
            }
        }

        [Theory]
        [InlineData("XXX", 1)]
        [InlineData("YYY", 1)]
        [InlineData("AAA", 0)]
        [InlineData("Description", 0)]
        [InlineData("Name", 2)]
        [InlineData("Excerpt", 2)]
        [InlineData("Excerpt Name", 2)]
        [InlineData("Excerpt Description", 0)] //AND
        [InlineData("Excerpt YYY", 1)] //AND
        [InlineData("Name Excerpt YYY", 1)] //AND
        public void ShouldGetSearchResults(string searchText, int expected)
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser().Build())
            {
                var seed0 = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = "Name contains XXX and other text",
                    Description = "Description contains AAA and other text",
                    Excerpt = "Excerpt contains YYY and other text",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.Public,
                };

                var seed1 = new Trail
                {
                    ID = Guid.NewGuid(),
                    Name = "Name Demo Track",
                    Description = "Description for Demo Track",
                    Excerpt = "Excerpt for Demo Track",
                    ListAccess = AccessLevel.Public,
                    GpxDownloadAccess = AccessLevel.Public
                };

                host.TrailContext.Trails.Add(seed1);
                host.TrailContext.Trails.Add(seed0);
                Assert.True(host.TrailContext.SaveChanges() > 1);
                var hc = host.CreateHomeController();
                HomeViewModel vm = new HomeViewModel { SearchText = searchText };
                var searchResult = hc.Search(vm).ToModel<HomeViewModel>();
                Assert.Equal(expected, searchResult.IndexTrails.Count);
            }
        }

        [Fact]
        public void ShouldGetAllItemsForAdmin()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var testTrails = TrailViewModelProvider.CreateInstance();

                host.InitWithViewModel(testTrails.All);

                using (var hc = host.CreateHomeController())
                {
                    var indexResult = hc.AllTrails(null).ToModel<HomeViewModel>();
                    Assert.Equal(testTrails.All.Length, indexResult.IndexTrails.Count);
                }
            }
        }

        [Fact]
        public void ShouldGetAllItemsForUser()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser().Build())
            {
                var testTrails = TrailViewModelProvider.CreateInstance();

                host.InitWithViewModel(testTrails.All);

                using (var hc = host.CreateHomeController())
                {
                    var indexResult = hc.AllTrails(null).ToModel<HomeViewModel>();
                    Assert.Equal(testTrails.UserTrailsCounter, indexResult.IndexTrails.Count);
                }
            }
        }

        [Fact]
        public void ShouldGetAllItemsForModerators()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsModerator().Build())
            {
                var testTrails = TrailViewModelProvider.CreateInstance();

                host.InitWithViewModel(testTrails.All);

                using (var hc = host.CreateHomeController())
                {
                    var indexResult = hc.AllTrails(null).ToModel<HomeViewModel>();
                    Assert.True(indexResult.Login.IsModerator);
                    Assert.False(indexResult.Login.IsAdmin);
                    Assert.Equal(testTrails.ModeratorTrailsCounter, indexResult.IndexTrails.Count);
                }
            }
        }
    }
}
