using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Trail365.Internal;
using Trail365.Seeds;
using Trail365.UnitTests;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.IntegrationTests
{
    public class AzureBlobServiceTest
    {
        //Skip.If stellt sicher, dass die Tests auf Maschinen wo kein Zugang zum Storage Account definiert ist NICHT fehlschlagen sondern als "übersprungen" (gelb) gekennzeichnet sind.
        //das Gelb im Test-Runner UI verwirrt zwar am Anfang, aber der Versuch die Tests einfach aus zu kommentieren ist kopmplett fehlgeschlagen, da der Compiler dann beim
        //Refaktorisieren des API's gar nicht angeschlagen hat und im nächsten Krisenfall die Tests komplett veraltet und nicht gewartet waren.
        //deshalb nur "Skip", damit zumindest die Compilerprüfungen IMMER stattfinden!

        private string[] GetTestFiles()
        {
            var folder = DirectoryHelper.GetOutputDirectory("GpxTracks", true);
            return folder.GetFiles("*.gpx").Select(fi => fi.FullName).ToArray();
        }

        private readonly ITestOutputHelper OutputHelper;

        public AzureBlobServiceTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void Should_Seed_Data()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                var testTrails = TrailViewModelProvider.CreateInstance();
                host.InitWithViewModel(testTrails.All);
            }
        }

        //[SkippableFact]
        //public void API_Evolution()
        //{
        //    Skip.If(Environment.ExpandEnvironmentVariables(WebHostExtension.CloudStorageEnvironmentVariable) == WebHostExtension.CloudStorageEnvironmentVariable); //skip if the unresolved variable is returned

        //    MapScrapper ms = new MapScrapper();

        //    using (var host = HostBuilder.Empty().UseTestOutputHelper(this.OutputHelper).UseCloudStorage(WebHostExtension.CloudStorageEnvironmentVariable, "uploads").Build())
        //    {
        //        InitWithViewModel(host, TrailProvider.All);

        //        foreach (var trail in host.TrailContext.Trails.ToList())
        //        {
        //                string encodedDownloadUri = System.Net.WebUtility.UrlEncode(trail.GpxUrl);
        //                UriBuilder builder = new UriBuilder("https://trailexplorer-qa.azurewebsites.net/Index");//?gpxsource=gpsies&gpxargument=gtvksuobbslyljqw&mode=snapshot&style=outdoor&debug=false")
        //                builder.Query = $"mode=snapshot&style=outdoor&debug=false&gpxsource={encodedDownloadUri}";
        //                var screenshotData = ms.ScreenshotAsync(builder.Uri,CancellationToken.None, host.Logger).GetAwaiter().GetResult();
        //                File.WriteAllBytes($@"c:\work\png\{trail.Name}.png", screenshotData);
        //        }

        //    }

        //}

        [SkippableFact]
        public void ShouldUploadGpxFromViewModelToAzure()
        {
            Skip.If(Environment.ExpandEnvironmentVariables(TestHostExtension.CloudStorageEnvironmentVariable) == TestHostExtension.CloudStorageEnvironmentVariable); //skip if the unresolved variable is returned
            var testTrails = TrailViewModelProvider.CreateInstance();
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseCloudStorage(TestHostExtension.CloudStorageEnvironmentVariable, "uploads").Build())
            {
                host.InitWithViewModel(testTrails.All);
                Assert.Equal(testTrails.All.Length, host.TrailContext.Trails.ToList().Count);
            }
        }

        [SkippableTheory]
        [InlineData("test1")]
        [InlineData("test2")]
        public void ShouldUploadAndOverwrite(string imageType)
        {
            Skip.If(Environment.ExpandEnvironmentVariables(TestHostExtension.CloudStorageEnvironmentVariable) == TestHostExtension.CloudStorageEnvironmentVariable); //skip if the unresolved variable is returned

            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseCloudStorage(TestHostExtension.CloudStorageEnvironmentVariable, "uploads").Build())
            {
                Assert.NotNull(host.BlobService);
                string fileContentAsText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
                string fileName = Path.GetTempFileName() + ".txt"; //txt for mime detection!
                File.WriteAllText(fileName, fileContentAsText);

                Guid g = Guid.NewGuid();
                ////Sonderfall: Update zu einem späteren Zeitpunkt. Die Guid ist die selbe, aber wir brauchen eine neue UplaodUrl da wir auch eine neue Berechtigunge benötigen!

                //1. upload!
                Uri absoluteDownloadUrl;
                using (Stream source = File.OpenRead(fileName))
                {
                    absoluteDownloadUrl = host.BlobService.Upload(g, imageType, Path.GetExtension(fileName), source, source.Length, host.RootUrl);
                }

                //1. Upload verifizieren
                var downloadUrl = absoluteDownloadUrl.ToString();  //uploader.GetResolvedUrl(relativeDownloadUrl.ToString());
                HttpClient cl1 = new HttpClient();
                using (var stream1 = cl1.GetStreamAsync(downloadUrl.ToString()).GetAwaiter().GetResult())
                {
                    using (var reader1 = new StreamReader(stream1))
                    {
                        string storageContent1 = reader1.ReadToEnd();
                        Assert.Equal(fileContentAsText, storageContent1);
                    }
                }

                //2. Upload mit anderem Content
                File.WriteAllText(fileName, "Dies ist kein Lorem ipsum dolor");

                Uri downloadUri2 = null;
                using (Stream source = File.OpenRead(fileName))
                {
                    downloadUri2 = host.BlobService.Upload(g, imageType, Path.GetExtension(fileName), source, source.Length, host.RootUrl);
                }

                //2. Upload verifizieren
                HttpClient cl2 = new HttpClient();
                using (var stream2 = cl2.GetStreamAsync(downloadUri2.ToString()).GetAwaiter().GetResult())
                {
                    using (var reader2 = new StreamReader(stream2))
                    {
                        string storageContent2 = reader2.ReadToEnd();
                        Assert.Equal("Dies ist kein Lorem ipsum dolor", storageContent2);
                    }
                }
                Assert.Equal(downloadUri2, absoluteDownloadUrl);
            }
        }

        [SkippableTheory]
        [InlineData("test3")]
        [InlineData("test4")]
        public void ShouldUpAndDownloadToAzure_DemoImages_AndDelete(string imageType)
        {
            Skip.If(Environment.ExpandEnvironmentVariables(TestHostExtension.CloudStorageEnvironmentVariable) == TestHostExtension.CloudStorageEnvironmentVariable); //skip if the unresolved variable is returned

            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseCloudStorage(TestHostExtension.CloudStorageEnvironmentVariable, "uploads").Build())
            {
                Assert.NotNull(host.BlobService);

                foreach (string demoImage in this.GetTestFiles())
                {
                    Guid g = Guid.NewGuid();
                    //Uri url = host.BlobService.CreateUploadUrl(g, imageType, demoImage, TimeSpan.FromMinutes(5), host.RootUrl);

                    Uri downloadUri = null;

                    using (Stream source = File.OpenRead(demoImage))
                    {
                        downloadUri = host.BlobService.Upload(g, imageType, Path.GetExtension(demoImage), source, source.Length, host.RootUrl);
                    }

                    HttpClient cl = new HttpClient();
                    using (var stream = cl.GetStreamAsync(downloadUri.ToString()).GetAwaiter().GetResult())
                    {
                        Assert.True(stream.CanRead);
                    }

                    host.BlobService.Remove(g, imageType, downloadUri.ToString());

                    var ex = Assert.Throws<System.Net.Http.HttpRequestException>(() =>
                    {
                        cl.GetStreamAsync(downloadUri.ToString()).GetAwaiter().GetResult();
                    });
                    Assert.Contains("404", ex.Message);
                }
            }
        }
    }
}
