using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using MSS.GraphQL.Facebook;
using Trail365.Seeds;
using Trail365.Services;
using Trail365.UnitTests.TestContext;
using Trail365.Web;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class FacebookEventImporterTest
    {
        private readonly ITestOutputHelper OutputHelper;

        private readonly DownloadService DownloadService;

        public FacebookEventImporterTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            var validDummyImage = System.IO.File.ReadAllBytes(Images.IATF2020AsJpg);
            this.DownloadService = new DownloadServiceSkeleton((url) => validDummyImage);
        }

        public FacebookEventDescriptor[] ToArray(params FacebookEventDescriptor[] items)
        {
            if (items == null) return new FacebookEventDescriptor[] { };
            return items.ToArray();
        }

        [Fact]
        public void ShouldHandleDuplicates()
        {
            var validDummyImage = System.IO.File.ReadAllBytes(Images.IATF2020AsJpg);
            DownloadService ds = new DownloadServiceSkeleton((url) => validDummyImage);
            var eventsFromFB = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<FacebookEventDescriptor>>(System.IO.File.ReadAllText(FBEvents.FBFurth));
            Assert.Equal(3, eventsFromFB.Count);

            var data = new FacebookEventData
            {
                ExternalSource = "sourceID",
                Events = eventsFromFB.ToArray(),
            };
            Assert.Equal(3, data.Events.Length);

            using (var host = TestHostBuilder.Empty().WithTrailContext().UseFileSystemStorage().Build())
            {
                var importer1 = host.CreateFacebookEventImporter(ds);
                importer1.Import(data, host.Logger);
                Assert.Single(importer1.InsertedPlaces);
                host.TrailContext.SaveChanges();

                var importer2 = host.CreateFacebookEventImporter(ds);
                importer2.Import(data, host.Logger);
                Assert.Empty(importer2.InsertedPlaces);
            }
        }

        [Fact]
        public void ShouldInsertAndUpdateWithCoverAndPlace()
        {
            DateTimeOffset start = new DateTimeOffset(1999, 12, 13, 14, 15, 00, TimeSpan.Zero);
            DateTimeOffset end = start.AddMinutes(120);

            FacebookEventDescriptor sample = new FacebookEventDescriptor
            {
                Name = "xyz",
                Description = "abc",
                StartTime = start,
                EndTime = end,
                Id = "myId",
                Cover = new Cover { ID = "123", Source = @"https://www.salzamt.at/picture1.png" },
                Place = new Place { Name = "P1", Location = new Location { City = "Entenhausen", Country = "Walt Disney" } }
            };

            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).WithTrailContext().UseFileSystemStorage().Build())
            {
                var importer = host.CreateFacebookEventImporter(this.DownloadService);

                importer.Import(new FacebookEventData
                {
                    ExternalSource = "SRC",
                    Events = this.ToArray(sample)
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Single(importer.InsertedEvents);
                Assert.Single(importer.InsertedImages);
                Assert.Single(importer.InsertedPlaces);

                host.TrailContext.SaveChanges();

                var item = host.TrailContext.Events.Where(e1 => e1.ExternalID == sample.Id).Single();

                Assert.Equal(item.CreatedUtc, start.UtcDateTime);
                Assert.False(item.ModifiedUtc.HasValue);

                importer = host.CreateFacebookEventImporter(this.DownloadService);

                sample.Name = "modifiedName";

                importer.Import(new FacebookEventData
                {
                    ExternalSource = "SRC",
                    Events = this.ToArray(sample)
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Single(importer.UpdatedEvents);
                Assert.Empty(importer.InsertedEvents);
                Assert.Empty(importer.InsertedImages);
                Assert.Empty(importer.InsertedPlaces);

                host.TrailContext.SaveChanges();

                item = host.TrailContext.Events.Where(e1 => e1.ExternalID == sample.Id).Single();

                Assert.Equal(item.CreatedUtc, start.UtcDateTime);
                Assert.True(item.ModifiedUtc.HasValue);
                Assert.Equal(importer.UtcNow, item.ModifiedUtc.Value);
            }
        }
    }
}
