using System;
using System.Linq;
using Newtonsoft.Json;
using Trail365.DTOs;
using Trail365.Seeds;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class ImageRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public ImageRouteTest(ITestOutputHelper helper)
        {
            OutputHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        [Fact]
        public void ShouldCreateImageViaDto()
        {
            var testData = ImageDtoProvider.CreateInstance();
            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseFileSystemStorage().WithTrailContext().UseStaticAuthenticationAsAdmin().Build())
            {
                var counter = 0;
                foreach (var seed in testData.All)
                {
                    counter += 1;
                    OutputHelper.WriteLine($"ImageID={seed.ID} ({counter}/{testData.All.Length})");

                    var json = JsonConvert.SerializeObject(seed);
                    var response = server.PostToServer("api/image", json).EnsureSuccessStatusCode();
                    string resultJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var image = Newtonsoft.Json.JsonConvert.DeserializeObject<BlobDto>(resultJson);
                    Assert.Equal(image.ID, seed.ID);
                }
                Assert.Equal(testData.All.Length, server.TrailContext.Blobs.ToList().Count);
            }
        }
    }
}
