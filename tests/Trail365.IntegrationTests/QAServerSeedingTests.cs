using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Graphics;
using Trail365.Seeds;
using Trail365.UnitTests;
using Xunit;

namespace Trail365.IntegrationTests
{
    public class QAServerSeedingTests : ExternalServerSeedingTests
    {
        public QAServerSeedingTests()
        {
            this.InstanceRootUrl = "https://trail365-qa.azurewebsites.net/";
        }

        public static string CreateTemplateMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# H1 Header Text");
            sb.AppendLine("Default text line");
            sb.AppendLine("## H2 Header Text");
            sb.AppendLine("Default text line");
            sb.AppendLine("- List element 1 on root level");
            sb.AppendLine("- List element 2 on root level");
            sb.AppendLine("    - List element 1 on  level 1");
            sb.AppendLine("    - List element 2 on  level 1");
            sb.AppendLine("Default text line");
            sb.AppendLine("### H3 Header Text");
            sb.AppendLine("Default text line");
            sb.AppendLine("Default text line with [Link to Google](https://www.google.com).");
            sb.AppendLine("Default text line");
            sb.AppendLine("Default text line with a badge: ![kasd](https://img.shields.io/badge/trail-easy-lightgreen) ");
            sb.AppendLine("");
            sb.AppendLine("### H3 Header after empty row and before table");
            sb.AppendLine("col1|col2|col3");
            sb.AppendLine("---|---|---");
            sb.AppendLine("value1|value2|value3");
            sb.AppendLine("ola|ole|oops");
            return sb.ToString();
        }

        public static readonly string LoremIpsum = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";

        public static readonly string LoremIpsumWithMarkDown = CreateTemplateMarkdown();

        [SkippableFact]
        public void ClearAllEventsOnQAServer()
        {
            Skip.If(SkipMode);
            base.ClearAllEventsOnServer();
        }

#if DEBUG
        private readonly bool SkipMode = true;
#else
        private readonly bool SkipMode = true;
#endif

        [SkippableFact]
        public void ClearAllTrailsOnQAServer()
        {
            Skip.If(SkipMode);
            base.ClearAllTrailsOnServer();
        }

        [SkippableFact]
        public void PushBasecampExportsToQAServer()
        {
            Skip.If(SkipMode);
            base.PushBasecampExportsToServer(@"C:\work\data\BasecampExports", "basecamp_qa");
        }

        public static readonly System.Drawing.Size Small = new System.Drawing.Size(460, 460);//http://tim-stanley.com/post/standard-web-digital-image-sizes/

        [SkippableFact]
        public void Push1KDemoTrailsToQAServerViaEntity()
        {
            Skip.If(SkipMode);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var testData = ImageDtoProvider.CreateInstance().All.First();

                for (int i = 0; i < 25; i++)
                {
                    var imageDto = new BlobDto();
                    var data = ImageResizer.Resize(testData.Data, Small);
                    imageDto.SubFolder = testData.SubFolder;
                    imageDto.Data = data;

                    var imgJson = JsonConvert.SerializeObject(imageDto);
                    var imgCnt = new StringContent(imgJson, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(this.ApiGetImageUrl(), imgCnt).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    var seed = new Trail
                    {
                        ID = Guid.NewGuid(),
                        Name = $"Demo Track {i}",
                        Description = LoremIpsumWithMarkDown,
                        InternalDescription = LoremIpsum,
                        ListAccess = AccessLevel.Public,
                        GpxDownloadAccess = AccessLevel.User,
                        PreviewImageID = imageDto.ID,
                    };

                    Assert.False(seed.GpxBlobID.HasValue);
                    var json = JsonConvert.SerializeObject(seed);
                    var cnt = new StringContent(json, Encoding.UTF8, "application/json");
                    client.PostAsync(this.ApiGetRawTrailsUrl(), cnt).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                }
            }
        }

        [SkippableFact]
        public void PushStorySeedsToQAServerViaDto()
        {
            Skip.If(SkipMode);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new UriBuilder(this.InstanceRootUrl).Uri;
                var storiesJson = client.GetStringAsync(this.ApiGetStoriesUrl()).GetAwaiter().GetResult();
                var stories = JsonConvert.DeserializeObject<List<StoryDto>>(storiesJson);
                var seedsProvider = StoryDtoProvider.RealisticStories();
                foreach (var seed in seedsProvider.All)
                {
                    var exists = stories.SingleOrDefault(t => t.ID == seed.ID) != null;
                    if (exists)
                    {
                        throw new NotImplementedException("NI");
                        //client.DeleteAsync(this.ApiGetSingleTrailUrl(seed.ID)).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                    }
                    client.PostStoryViaApi(seed).EnsureSuccessStatusCode();
                }
            }
        }

        [SkippableFact]
        public void PushTrailSeedsToQAServerViaDto()
        {
            Skip.If(SkipMode);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new UriBuilder(this.InstanceRootUrl).Uri;
                var trailsJson = client.GetStringAsync(this.ApiGetTrailsUrl()).GetAwaiter().GetResult();
                var trails = JsonConvert.DeserializeObject<List<TrailDto>>(trailsJson);
                foreach (var seed in TrailDtoProvider.CreateInstance().All)
                {
                    seed.Description = LoremIpsumWithMarkDown;
                    seed.InternalDescription = LoremIpsum;
                    seed.Excerpt = LoremIpsum;

                    var exists = trails.SingleOrDefault(t => t.ID == seed.ID) != null;

                    if (exists)
                    {
                        client.DeleteAsync(this.ApiGetSingleTrailUrl(seed.ID)).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                    }

                    client.PostTrailViaApi(seed).EnsureSuccessStatusCode();
                }
            }
        }

        private void UpsertEvents(EventDtoProvider seedsProvider)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new UriBuilder(this.InstanceRootUrl).Uri;
                var eventsJson = client.GetStringAsync(this.ApiGetEventsUrl()).GetAwaiter().GetResult();

                var events = JsonConvert.DeserializeObject<List<EventDto>>(eventsJson);

                foreach (var seed in seedsProvider.All)
                {
                    var exists = events.SingleOrDefault(t => t.ID == seed.ID) != null;

                    if (exists)
                    {
                        client.DeleteAsync(this.ApiGetSingleEventUrl(seed.ID)).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                    }
                    client.PostEventViaApi(seed).EnsureSuccessStatusCode();
                }
            }
        }

        [SkippableFact]
        public void Facebook2Trail365()
        {
            Skip.If(SkipMode);
            base.ExtractFromFacebookToTrail365(DateTime.UtcNow.AddDays(-25));
        }

        [SkippableFact]
        public void PushTRVEvents2020EventSeedsToQAServerViaDto()
        {
            Skip.If(SkipMode);
            this.UpsertEvents(EventDtoProvider.CreateTRVEvents2020());
        }

        [SkippableFact]
        public void PushMarkdownEventSeedsToQAServerViaDto()
        {
            Skip.If(SkipMode);
            this.UpsertEvents(EventDtoProvider.CreateFromEventDtos(EventDtoProvider.MarkdownDemo()));
        }
    }
}
