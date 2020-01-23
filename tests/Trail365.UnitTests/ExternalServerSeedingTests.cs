using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using MSS.GraphQL.Facebook;
using Newtonsoft.Json;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Services;
using Xunit;

namespace Trail365.UnitTests
{
    public abstract class ExternalServerSeedingTests
    {
        public string InstanceRootUrl { get; set; }
        public string FacebookToken { get; set; }
        public string FacebookID { get; set; }

        public string ApiGetEventsUrl() => this.InstanceRootUrl + RouteName.EventsApi;

        public string ApiGetTrailsUrl() => this.InstanceRootUrl + RouteName.TrailsApi;

        public string ApiGetStoriesUrl() => this.InstanceRootUrl + RouteName.StoriesApi;

        public string ApiGetRawTrailsUrl() => this.InstanceRootUrl + @"api/trail/raw";

        public string ApiGetImageUrl() => this.InstanceRootUrl + @"api/image";

        public string ApiGetSingleTrailUrl(Guid id) => this.InstanceRootUrl + $"{RouteName.TrailsApi}/{id}";

        public string ApiGetSingleEventUrl(Guid id) => this.InstanceRootUrl + $"{RouteName.EventsApi}/{id}";

        public string ApiCalculatePreviewUrl(Guid id) => this.InstanceRootUrl + $"{RouteName.TrailsApi}/preview/{id}";

        protected void ClearAllEventsOnServer()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var eventsJson = client.GetStringAsync(this.ApiGetEventsUrl()).GetAwaiter().GetResult();
                var events = JsonConvert.DeserializeObject<List<EventDto>>(eventsJson);

                foreach (var seed in events)
                {
                    client.DeleteAsync(this.ApiGetSingleEventUrl(seed.ID)).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                }

                eventsJson = client.GetStringAsync(this.ApiGetEventsUrl()).GetAwaiter().GetResult();
                events = JsonConvert.DeserializeObject<List<EventDto>>(eventsJson);
                Assert.Empty(events);
            }
        }

        protected void PushBasecampExportsToServer(string directory, string source)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new UriBuilder(this.InstanceRootUrl).Uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var seedData = System.IO.Directory.GetFiles(directory, "*.gpx");

                foreach (var gpxFile in seedData)
                {
                    var response = client.PostMultiTrailGpxViaApi(gpxFile, source);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        protected void ClearAllTrailsOnServer()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var trailsJson = client.GetStringAsync(this.ApiGetTrailsUrl()).GetAwaiter().GetResult();
                var trails = JsonConvert.DeserializeObject<List<TrailDto>>(trailsJson);

                foreach (var seed in trails)
                {
                    client.DeleteAsync(this.ApiGetSingleTrailUrl(seed.ID)).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                }

                trailsJson = client.GetStringAsync(this.ApiGetTrailsUrl()).GetAwaiter().GetResult();
                trails = JsonConvert.DeserializeObject<List<TrailDto>>(trailsJson);
                Assert.Empty(trails);
            }
        }

        private void UpsertFacebookEvents(FacebookEventDescriptor[] fbEvents)
        {
            string externalSource = $"fb-{this.FacebookID.ToLowerInvariant().Trim()}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new UriBuilder(this.InstanceRootUrl).Uri;
                //quick solution: retrieve ALL Events from server and filter locally!
                var eventsJson = client.GetStringAsync(this.ApiGetEventsUrl()).GetAwaiter().GetResult();

                var events = JsonConvert.DeserializeObject<List<EventDto>>(eventsJson);

                foreach (FacebookEventDescriptor seed in fbEvents)
                {
                    var exists = events.SingleOrDefault(t => t.ExternalSource == externalSource && t.ExternalID == seed.Id);

                    if (exists != null)
                    {
                        client.DeleteAsync(this.ApiGetSingleEventUrl(exists.ID)).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                    }

                    if (seed.IsDraft) continue;
                    EventDto dto = new EventDto
                    {
                        Name = seed.Name,
                        StartTimeUtc = seed.StartTime.UtcDateTime,
                        EndTimeUtc = seed.EndTime.UtcDateTime,
                        Description = seed.Description,
                        ExternalSource = externalSource,
                        ExternalID = seed.Id
                    };

                    if (seed.Cover != null)
                    {
                        BlobDto extractedCover = HttpClientDownloadService.DefaultInstance.DownloadFromUrl(seed.Cover.Source);
                        dto.CoverImage = extractedCover;
                    }

                    if (seed.Place != null)
                    {
                        dto.Place = new PlaceDto
                        {
                            Name = seed.Place.Name
                        };

                        if (seed.Place.Location != null)
                        {
                            dto.Place.Country = seed.Place.Location.Country;
                            dto.Place.Latitude = seed.Place.Location.Latitude;
                            dto.Place.Longitude = seed.Place.Location.Longitude;
                        }
                    }

                    if (exists != null)
                    {
                        dto.CreatedUtc = exists.CreatedUtc;
                    }

                    if (seed.StartTime < DateTimeOffset.Now.AddDays(-3))
                    {
                        dto.CreatedUtc = seed.StartTime.UtcDateTime;
                    }
                    dto.ListAccess = AccessLevel.Public;

                    client.PostEventViaApi(dto).EnsureSuccessStatusCode();
                }
            }
        }

        protected void ExtractFromFacebookToTrail365(DateTime sinceUtc)
        {
            FacebookDataExtractor extractor = new FacebookDataExtractor();
            DateTimeOffset inFBSince = new DateTimeOffset(sinceUtc);
            var results = extractor.GetEventDescriptionsByOwnerId(this.FacebookToken, this.FacebookID, inFBSince, CancellationToken.None).ToArray();
            //this.UpsertFacebookEvents(results);
            var json = JsonConvert.SerializeObject(results);
            System.Diagnostics.Debug.WriteLine(json);
        }
    }
}
