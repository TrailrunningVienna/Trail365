using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrailControllerTest
    {
        [Fact]
        public void ShouldCreateController_NotLoggedIn()
        {
            using (var host = TestHostBuilder.Empty().UseStaticAuthenticationAsNotLoggedIn().WithTrailContext().Build())
            {
                using (var hc = host.CreateTrailController())
                {
                    Assert.NotNull(hc);
                    Assert.NotNull(hc.Url);
                }
            }
        }

        [Fact]
        public void ShouldHandleUploadController()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                using (Stream t1 = File.OpenRead(GpxTracks.Buschberg))
                {
                    using (var hc = host.CreateTrailController())
                    {
                        Guid trailID = Guid.NewGuid();
                        IFormFile file = new FormFile(t1, 0, t1.Length, "busch", "buschberg.gpx");
                        var result = hc.UploadGpx(trailID, file).ToModel<CreateTrailViewModel>();
                        Assert.Equal(result.ID, trailID);
                        hc.SaveGpx(result);
                    }
                }
            }
        }

        [Fact]
        public void ShouldReturnCreateTrailViewModel()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                using (var hc = host.CreateTrailController())
                {
                    var viewModel = hc.CreateTrail().ToModel<CreateTrailViewModel>();
                }
            }
        }

        [Fact]
        public void ShouldCreateTrail()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                using (var hc = host.CreateTrailController())
                {
                    //use http-get to initialize the ViewModel
                    var viewModel = hc.CreateTrail().ToModel<CreateTrailViewModel>();

                    viewModel.Name = "sample";

                    //use http-post to create the trail via ViewModel
                    hc.SaveGpx(viewModel);
                }
            }
        }
    }
}
