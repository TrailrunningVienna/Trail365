using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Trail365.DTOs;
using Trail365.Seeds;
using Trail365.Services;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class FileSystemBlobServiceTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public FileSystemBlobServiceTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        public static Stream CreateTestContent(string text)
        {
            var ms = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(text))
            {
                Position = 0
            };
            return ms;
        }

        [Fact]
        public void ShouldUploadSingleFile()
        {
            string testFolder = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString("N"));
            var service = new FileSystemBlobService(testFolder, "/blob");
            Assert.Empty(Directory.GetFiles(testFolder, "*.*", SearchOption.AllDirectories));
            Guid fileID = Guid.NewGuid();
            using (var sourceStream = CreateTestContent("abcdefgQWERT"))
            {
                var uri = service.Upload(fileID, "txt", "txt", sourceStream, sourceStream.Length, HelperExtensions.EmptyUrlHelper);
                Assert.NotNull(uri);
                Assert.False(uri.IsFile);
            }
            Assert.Single(Directory.GetFiles(testFolder, "*.txt", SearchOption.AllDirectories));
            Assert.Single(Directory.GetFiles(testFolder, "*.*", SearchOption.AllDirectories));
        }

        [Fact]
        public async Task ShouldProvideImageDownloads()
        {
            var testData = ImageDtoProvider.CreateInstance();
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseStaticAuthenticationAsUser().WithTrailContext().UseFileSystemStorage(@"/blob").Build()) //implicite fs blob
            {
                foreach (var imageDto in testData.All)
                {
                    Assert.NotNull(imageDto.Data);
                    var json = JsonConvert.SerializeObject(imageDto);
                    var response = host.PostToServer("api/image", json).EnsureSuccessStatusCode();
                    string resultJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var resultImageDto = Newtonsoft.Json.JsonConvert.DeserializeObject<BlobDto>(resultJson);
                    Assert.Equal(resultImageDto.ID, imageDto.ID);
                    Assert.False(string.IsNullOrEmpty(resultImageDto.Url));
                    using (var client = host.CreateClient())
                    {
                        Assert.StartsWith(client.BaseAddress.ToString().ToLowerInvariant(), resultImageDto.Url.ToLowerInvariant());
                        var resp = client.GetAsync(resultImageDto.Url).GetAwaiter().GetResult();
                        resp.EnsureSuccessStatusCode();
                        var contentbytes = await resp.Content.ReadAsByteArrayAsync();
                        Assert.NotNull(contentbytes);
                        Assert.NotEmpty(contentbytes);
                        Assert.NotNull(imageDto.Data);
                        Assert.Equal(imageDto.Data.Length, contentbytes.Length);
                    }
                }
                Assert.Equal(testData.All.Length, host.TrailContext.Blobs.ToList().Count);
            }
        }
    }
}
