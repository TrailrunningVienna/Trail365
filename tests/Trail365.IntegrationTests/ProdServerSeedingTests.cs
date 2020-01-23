using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Trail365.DTOs;
using Trail365.Seeds;
using Trail365.UnitTests;
using Xunit;

namespace Trail365.IntegrationTests
{
    public class ProdServerSeedingTests : ExternalServerSeedingTests
    {
        public ProdServerSeedingTests()
        {
            this.InstanceRootUrl = "https://trail365.azurewebsites.net/";
        }

#if DEBUG
        private readonly bool SkipMode = true;
#else
        private readonly bool SkipMode = true;
#endif

        [SkippableFact]
        public void ClearAllOnPRODServer()
        {
            Skip.If(SkipMode);
            base.ClearAllEventsOnServer();
            base.ClearAllTrailsOnServer();
        }

        [SkippableFact]
        public void ClearAllEventsOnPRODServer()
        {
            Skip.If(SkipMode);
            base.ClearAllEventsOnServer();
        }

        [SkippableFact]
        public void ExtractFromFacebook()
        {
            Skip.If(SkipMode);
            base.ExtractFromFacebookToTrail365(DateTime.UtcNow.AddYears(-1));
        }

        [SkippableFact]
        public void ClearAllTrailsOnPRODServer()
        {
            Skip.If(SkipMode);
            base.ClearAllTrailsOnServer();
        }

        [SkippableFact]
        public void PushBasecampExportsToPRODServer()
        {
            Skip.If(SkipMode);
            base.PushBasecampExportsToServer(@"C:\work\data\BasecampExports", "basecamp");
        }

        [SkippableFact]
        public void RecalculatePreviewsOnAllTrailsOnPRODServer()
        {
            Skip.If(SkipMode);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var trailsJson = client.GetStringAsync(this.ApiGetTrailsUrl()).GetAwaiter().GetResult();
                var trails = JsonConvert.DeserializeObject<List<TrailDto>>(trailsJson);

                foreach (var seed in trails)
                {
                    string url = this.ApiCalculatePreviewUrl(seed.ID);
                    client.GetAsync(url).GetAwaiter().GetResult().EnsureSuccessStatusCode();
                }
            }
        }

        [SkippableFact]
        public void PushSeedsToProdServerViaDto()
        {
            Skip.If(SkipMode);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new UriBuilder(this.InstanceRootUrl).Uri;
                var trailsJson = client.GetStringAsync(this.ApiGetTrailsUrl()).GetAwaiter().GetResult();
                var trails = JsonConvert.DeserializeObject<List<TrailDto>>(trailsJson);
                foreach (var seed in TrailDtoProvider.CreateProductionSeeds(@"c:\PS\HQTracks1").All)
                {
                    client.PostTrailViaApi(seed).EnsureSuccessStatusCode();
                }
            }
        }
    }
}
