using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Trail365.DTOs;

namespace Trail365
{
    public static class HttpClientExtension
    {
        public static HttpResponseMessage GetTrailsViaApi(this HttpClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.BaseAddress == null) throw new InvalidOperationException("BaseAddress must be set");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string url = RouteName.TrailsApi;
            var result = client.GetAsync(url).GetAwaiter().GetResult();
            return result;
        }

        public static HttpResponseMessage PostMultiTrailGpxViaApi(this HttpClient client, string fileName, string source)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.BaseAddress == null) throw new InvalidOperationException("BaseAddress must be set");

            HttpContent fileStreamContent = new StreamContent(System.IO.File.OpenRead(fileName));
            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            string sourceEncoded = System.Net.WebUtility.UrlEncode(source);
            string url = $"{RouteName.TrailsApi}/create?source={sourceEncoded}";
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(fileStreamContent);
                return client.PostAsync(url, formData).GetAwaiter().GetResult();
            }
        }

        public static HttpResponseMessage PostStoryViaApi(this HttpClient client, StoryDto story)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.BaseAddress == null) throw new InvalidOperationException("BaseAddress must be set");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonConvert.SerializeObject(story);
            var cnt = new StringContent(json, Encoding.UTF8, "application/json");
            string url = $"{RouteName.StoriesApi}";
            var result = client.PostAsync(url, cnt).GetAwaiter().GetResult();
            return result;
        }

        public static HttpResponseMessage PostTrailViaApi(this HttpClient client, TrailDto trail)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.BaseAddress == null) throw new InvalidOperationException("BaseAddress must be set");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonConvert.SerializeObject(trail);
            var cnt = new StringContent(json, Encoding.UTF8, "application/json");
            string url = $"{RouteName.TrailsApi}";
            var result = client.PostAsync(url, cnt).GetAwaiter().GetResult();
            return result;
        }

        public static HttpResponseMessage PostEventViaApi(this HttpClient client, EventDto eventDto)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.BaseAddress == null) throw new InvalidOperationException("BaseAddress must be set");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonConvert.SerializeObject(eventDto);
            var cnt = new StringContent(json, Encoding.UTF8, "application/json");
            string url = RouteName.EventsApi;
            var result = client.PostAsync(url, cnt).GetAwaiter().GetResult();
            return result;
        }
    }
}
