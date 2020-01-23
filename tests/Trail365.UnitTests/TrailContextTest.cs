using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.Services;
using Trail365.Web;
using Xunit;
using Xunit.Abstractions;
using FB = MSS.GraphQL.Facebook;


namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrailContextTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TrailContextTest(ITestOutputHelper outputHelper)
        {
            this.OutputHelper = outputHelper;
        }



        [Fact]
        public void ShouldSeedSingleTrail()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Trails);
                Assert.Empty(host.TrailContext.Blobs);
                var seedDto = TrailDtoProvider.CreateUniqueTrail();
                host.TrailContext.SeedTrails(host.BlobService, new DTOs.TrailDto[] { seedDto }, host.RootUrl);
                host.TrailContext.SaveChanges();
                var allBlobs = host.TrailContext.Blobs.ToList();
                Assert.Single(allBlobs);

                var trail = host.TrailContext.GetTrailsByFilter(TrailQueryFilter.GetByID(seedDto.ID, true)).Single();
                Assert.True(trail.GpxBlobID.HasValue);
                Assert.NotNull(trail.GpxBlob);

            }
        }


        [Fact]
        public void ShouldSeedTrails()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Trails);
                Assert.Empty(host.TrailContext.Blobs);
                var seedProvider = TrailDtoProvider.CreateInstanceForPublicSeeds();
                host.TrailContext.SeedTrails(host.BlobService, seedProvider.All, host.RootUrl);
                host.TrailContext.SaveChanges();
                var allBlobs = host.TrailContext.Blobs.ToList();

                Assert.Equal(seedProvider.All.Length, allBlobs.Count);
                allBlobs.ForEach(b => Assert.False(string.IsNullOrEmpty(b.FolderName)));
                allBlobs.ForEach(b => Assert.False(string.IsNullOrEmpty(b.MimeType)));
                allBlobs.ForEach(b => Assert.True(b.StorageSize.HasValue));
                allBlobs.ForEach(b => Assert.False(string.IsNullOrEmpty(b.ContentHash)));
            }
        }

        [Fact]
        public void EnsureRFIOnPlaceDeletion_Trails_Allowed()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Trails);
                Assert.Empty(host.TrailContext.Places);

                DateTime now = DateTime.UtcNow;
                var seed1 = new Place
                {
                    Name = "test1",
                    Longitude = 1,
                    Latitude = 1,
                    Country = "Austria1"
                };

                var seed2 = new Place
                {
                    Name = "test2",
                    Longitude = 1,
                    Latitude = 1,
                    Country = "Austria2"
                };

                Trail entity = new Trail
                {
                    Name = now.ToString(),
                    ListAccess = AccessLevel.Public,
                    StartPlace = null,
                    EndPlace = null
                };

                host.TrailContext.Trails.Add(entity);
                host.TrailContext.Places.Add(seed1);
                host.TrailContext.Places.Add(seed2);
                host.TrailContext.SaveChanges();

                Assert.Single(host.TrailContext.Trails);
                Assert.Equal(2, host.TrailContext.Places.ToList().Count);

                var reloadedTrail = host.TrailContext.GetTrails(true, false).Single();

                Assert.Null(reloadedTrail.StartPlace);
                Assert.Null(reloadedTrail.EndPlace);

                host.TrailContext.DeletePlace(seed1);
                host.TrailContext.DeletePlace(seed2);

                var changes = host.TrailContext.SaveChanges();
                Assert.True((changes > 0));
                Assert.Single(host.TrailContext.Trails);
                Assert.Empty(host.TrailContext.Places);
                var checkedLoad = host.TrailContext.GetTrails(true, false).Single();
                Assert.Null(checkedLoad.StartPlace);
                Assert.Null(checkedLoad.EndPlace);
            }
        }

        [Fact]
        public void EnsureRFIOnPlaceDeletion_Trails_NotAllowed()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Trails);
                Assert.Empty(host.TrailContext.Places);

                DateTime now = DateTime.UtcNow;
                var seed1 = new Place
                {
                    Name = "test1",
                    Longitude = 1,
                    Latitude = 1,
                    Country = "Austria1"
                };

                var seed2 = new Place
                {
                    Name = "test2",
                    Longitude = 1,
                    Latitude = 1,
                    Country = "Austria2"
                };

                Trail entity = new Trail
                {
                    Name = now.ToString(),
                    ListAccess = AccessLevel.Public,
                    StartPlace = seed1,
                    EndPlace = seed2
                };

                host.TrailContext.Trails.Add(entity);
                host.TrailContext.Places.Add(seed1);
                host.TrailContext.Places.Add(seed2);
                host.TrailContext.SaveChanges();

                Assert.Single(host.TrailContext.Trails);
                Assert.Equal(2, host.TrailContext.Places.ToList().Count);

                var reloadedTrail = host.TrailContext.GetTrails(true, false).Single();
                Assert.NotNull(reloadedTrail.StartPlace);
                Assert.Equal(reloadedTrail.StartPlace.ID, seed1.ID);
                Assert.Equal(reloadedTrail.StartPlaceID, seed1.ID);

                Assert.NotNull(reloadedTrail.EndPlace);
                Assert.Equal(reloadedTrail.EndPlace.ID, seed2.ID);
                Assert.Equal(reloadedTrail.EndPlaceID, seed2.ID);

                Assert.Throws<InvalidOperationException>(() => host.TrailContext.DeletePlace(reloadedTrail.StartPlace));
                Assert.Throws<InvalidOperationException>(() => host.TrailContext.DeletePlace(reloadedTrail.EndPlace));
            }
        }

        [Fact]
        public void EnsureRFIOnPlaceDeletion_Events_DeletionAllowed()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Events);
                Assert.Empty(host.TrailContext.Places);

                DateTime now = DateTime.UtcNow;
                var seed = new Place
                {
                    Name = "test",
                    Longitude = 1,
                    Latitude = 1,
                    Country = "Austria"
                };

                Event entity = new Event
                {
                    Name = now.ToString(),
                    StartTimeUtc = now,
                    ListAccess = AccessLevel.Public,
                    Place = null, //NOT connected => no dependency
                };

                host.TrailContext.Events.Add(entity);
                host.TrailContext.Places.Add(seed);

                host.TrailContext.SaveChanges();

                Assert.Single(host.TrailContext.Events);
                Assert.Single(host.TrailContext.Places);
                var reloaded = host.TrailContext.GetEvents(true, false, false).Single();
                Assert.Null(reloaded.Place);

                host.TrailContext.DeletePlace(seed);
                var changes = host.TrailContext.SaveChanges();
                Assert.True((changes > 0));
                Assert.Single(host.TrailContext.Events);
                Assert.Empty(host.TrailContext.Places);
            }
        }

        [Fact]
        public void EnsureRFIOnPlaceDeletion_Events_DeletionNotAllowed()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Events);
                Assert.Empty(host.TrailContext.Places);

                DateTime now = DateTime.UtcNow;
                var seed = new Place
                {
                    Name = "test",
                    Longitude = 1,
                    Latitude = 1,
                    Country = "Austria"
                };

                Event entity = new Event
                {
                    Name = now.ToString(),
                    StartTimeUtc = now,
                    ListAccess = AccessLevel.Public,
                    Place = seed,
                };

                host.TrailContext.Events.Add(entity);
                host.TrailContext.Places.Add(seed);

                host.TrailContext.SaveChanges();

                Assert.Single(host.TrailContext.Events);
                Assert.Single(host.TrailContext.Places);
                var reloaded = host.TrailContext.GetEvents(true, false, false).Single();
                Assert.NotNull(reloaded.Place);
                Assert.Equal(reloaded.Place.ID, seed.ID);
                Assert.Equal(reloaded.PlaceID, seed.ID);

                Assert.Throws<InvalidOperationException>(() => host.TrailContext.DeletePlace(reloaded.Place));
            }
        }

        [Fact]
        public void ShouldImportFBEventsFromFile_Furt()
        {
            var eventsFromFB = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<FB.FacebookEventDescriptor>>(System.IO.File.ReadAllText(FBEvents.FBFurth));

            var validDummyImage = System.IO.File.ReadAllBytes(Images.IATF2020AsJpg);

            DownloadService ds = new DownloadServiceSkeleton((url) => validDummyImage);

            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                var importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "sourceID",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Equal(3, importer.InsertedEvents.Count);
                Assert.Equal(3, importer.InsertedImages.Count);
                Assert.Single(importer.InsertedPlaces);

                Assert.Empty(importer.UpdatedEvents);
                var changes = host.TrailContext.SaveChanges();
                Assert.Equal(7, changes);

                //import again THE SAME EVENTS
                importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "sourceID",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Empty(importer.InsertedEvents);
                Assert.Empty(importer.InsertedImages);
                Assert.Empty(importer.InsertedPlaces);
                Assert.Empty(importer.UpdatedEvents);
                var changes2 = host.TrailContext.SaveChanges();
                Assert.Equal(0, changes2);

                //import again THE SAME EVENTS but with a different SourceID
                importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "otherSource",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Equal(3, importer.InsertedEvents.Count);
                Assert.Equal(3, importer.InsertedImages.Count);
                Assert.Single(importer.InsertedPlaces);

                var changes3 = host.TrailContext.SaveChanges();
                Assert.Equal(7, changes3);
                //import by changing the name!
            }
        }

        [Fact]
        public void ShouldImportFBEventsFromFile()
        {
            var eventsFromFB = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<FB.FacebookEventDescriptor>>(System.IO.File.ReadAllText(FBEvents.FB01112019));

            var validDummyImage = System.IO.File.ReadAllBytes(Images.IATF2020AsJpg);

            DownloadService ds = new DownloadServiceSkeleton((url) => validDummyImage);

            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                var importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "sourceID",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Equal(402, importer.InsertedEvents.Count);
                Assert.Equal(402, importer.InsertedImages.Count);
                Assert.Equal(69, importer.InsertedPlaces.Count);

                Assert.Empty(importer.UpdatedEvents);
                var changes = host.TrailContext.SaveChanges();
                Assert.Equal(873, changes);

                //import again THE SAME EVENTS
                importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "sourceID",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Empty(importer.InsertedEvents);
                Assert.Empty(importer.InsertedImages);
                Assert.Empty(importer.InsertedPlaces);
                Assert.Empty(importer.UpdatedEvents);
                var changes2 = host.TrailContext.SaveChanges();
                Assert.Equal(0, changes2);

                //import again THE SAME EVENTS but with a different SourceID
                importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "otherSource",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Equal(402, importer.InsertedEvents.Count);
                Assert.Equal(402, importer.InsertedImages.Count);
                Assert.Single(importer.InsertedPlaces);

                var changes3 = host.TrailContext.SaveChanges();
                Assert.Equal(805, changes3);
                //import by changing the name!
            }
        }

        [Fact]
        public void ShouldImportFBEventsFromFileAsCanceled()
        {
            var eventsFromFB = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<MSS.GraphQL.Facebook.FacebookEventDescriptor>>(System.IO.File.ReadAllText(FBEvents.FB01112019));

            var validDummyImage = System.IO.File.ReadAllBytes(Images.IATF2020AsJpg);

            DownloadService ds = new DownloadServiceSkeleton((url) => validDummyImage);

            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                var importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "sourceID",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Equal(402, importer.InsertedEvents.Count);
                Assert.Equal(402, importer.InsertedImages.Count);
                Assert.Equal(69, importer.InsertedPlaces.Count);
                Assert.Empty(importer.UpdatedEvents);
                var changes = host.TrailContext.SaveChanges();
                Assert.Equal(873, changes);

                //import again THE SAME EVENTS BUT Canceled!
                eventsFromFB.ForEach(fb => fb.IsCanceled = true);
                importer = new FacebookEventImporter(host.TrailContext, host.RootUrl, ds, host.BlobService);
                importer.Import(new FacebookEventData
                {
                    ExternalSource = "sourceID",
                    Events = eventsFromFB.ToArray(),
                }, NullLogger.Instance).GetAwaiter().GetResult();

                Assert.Empty(importer.InsertedEvents);
                Assert.Empty(importer.InsertedImages);
                Assert.Empty(importer.InsertedPlaces);
                Assert.Equal(402, importer.UpdatedEvents.Count);
                var changes2 = host.TrailContext.SaveChanges();
                Assert.Equal(402, changes2);
            }
        }

        [Fact]
        public void PermissionsTest()
        {
            var testTrails = TrailEntityProvider.CreateInstance();

            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Trails);
                host.TrailContext.Trails.AddRange(testTrails.All);
                host.TrailContext.SaveChanges();
                Assert.Equal(testTrails.All.Length, host.TrailContext.Trails.Count());

                Assert.Equal(testTrails.PublicTrailsCounter, host.TrailContext.GetTrailsByListAccessFilter(AccessLevel.Public).Count());
                Assert.Equal(testTrails.UserTrailsCounter, host.TrailContext.GetTrailsByListAccessFilter(AccessLevel.User).Count());
                Assert.Equal(testTrails.UserTrailsCounter + testTrails.PublicTrailsCounter, host.TrailContext.GetTrailsByListAccessFilter(AccessLevel.User, AccessLevel.Public).Count());
            }
        }

        [Fact]
        public void EnsureUniqueResultOnEventStreams_Single_ShortDistance()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Events);
                DateTime now = DateTime.UtcNow;
                DateTime yesterday = now.AddDays(-1);
                DateTime tomorrow = now.AddDays(+1);
                Event seed = new Event
                {
                    Name = yesterday.ToString(),
                    StartTimeUtc = tomorrow,
                    CreatedUtc = yesterday,
                    ModifiedUtc = yesterday,
                    ListAccess = AccessLevel.Public,
                };
                host.TrailContext.Events.Add(seed);
                host.TrailContext.SaveChanges();
                Assert.Single(host.TrailContext.Events);
                var list = host.TrailContext.GetEventStreamForNews(0, 99, now, AccessLevel.Public);
                Assert.Single(list);
                list = host.TrailContext.GetEventStreamForNews(99, 0, now, AccessLevel.Public);
                Assert.Single(list);
                list = host.TrailContext.GetEventStreamForNews(99, 99, now, AccessLevel.Public);
                Assert.Single(list);
            }
        }

        [Fact]
        public void EnsureUniqueResultOnEventStreams_Single_LongDistance()
        {
            using (var host = TestHostBuilder.Empty().WithTrailContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                Assert.Empty(host.TrailContext.Events);
                DateTime now = DateTime.UtcNow;
                DateTime past = now.AddDays(-17);
                DateTime future = now.AddDays(+3);
                Event seed = new Event
                {
                    Name = past.ToString(),
                    StartTimeUtc = future,
                    CreatedUtc = past,
                    ModifiedUtc = null,
                    ListAccess = AccessLevel.Public,
                };
                host.TrailContext.Events.Add(seed);
                host.TrailContext.SaveChanges();
                Assert.Single(host.TrailContext.Events);
                var list = host.TrailContext.GetEventStreamForNews(0, 99, now, AccessLevel.Public);
                Assert.Single(list);
                list = host.TrailContext.GetEventStreamForNews(99, 0, now, AccessLevel.Public);
                Assert.Single(list);
                list = host.TrailContext.GetEventStreamForNews(99, 99, now, AccessLevel.Public);
                Assert.Single(list);
            }
        }
    }
}
