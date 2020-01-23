using System.Linq;
using Trail365.DTOs;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Xunit;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "BuildVerification")]
    public class ImageControllerTest
    {
        [Fact]
        public void ShouldCreateImages()
        {
            var testData = ImageDtoProvider.CreateInstance();
            using (var host = TestHostBuilder.Empty().UseFileSystemStorage().WithTrailContext().Build())
            {
                var hc = host.CreateImageController();
                foreach (var image in testData.All)
                {
                    var t = hc.PostImage(image);
                    var img = t.ToModel<BlobDto>();
                    Assert.Equal(img.ID, image.ID);
                }
                Assert.Equal(testData.All.Length, host.TrailContext.Blobs.ToList().Count);
            }
        }
    }
}
