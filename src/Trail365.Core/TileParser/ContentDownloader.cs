using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TrackExplorer.Core
{
    // API Design: some async pattern implemented because the class is handling resources/tasks where async make sense (lot of waitings)


    public class ContentDownloader
    {

        public ContentDownloader() : this(new HttpClient())
        {
        }

        public ContentDownloader(HttpClient client)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private readonly HttpClient Client;


        public async Task<byte[]> GetFromUriAsync(Uri uri, CancellationToken cancellation = default)
        {
            using var response = await this.GetResponseMessageFromUriAsync(uri, cancellation);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        protected Task<HttpResponseMessage> GetResponseMessageFromUriAsync(Uri uri, CancellationToken cancellation)
        {
            string requestUrl = uri.ToString();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            return this.Client.SendAsync(message, cancellation);
        }

        /// <summary>
        /// Try means: return false if resource does not exists, every other exception is returned as exception!
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="task"></param>
        /// <returns>false if uri does not exists!</returns>
        public bool TryGetContentFromUri(Uri uri, CancellationToken cancellationToken, out Task<byte[]> task)
        {

            if (uri.IsFile)
            {
                if (File.Exists(uri.AbsolutePath))
                {
                    task = System.IO.File.ReadAllBytesAsync(uri.AbsolutePath, cancellationToken);
                    return true;
                }
                else
                {
                    task = null;
                    return false;
                }
            }
            else
            {
                var resp = this.GetResponseMessageFromUriAsync(uri, cancellationToken).GetAwaiter().GetResult();
                if (resp.IsSuccessStatusCode)
                {
                    task = resp.Content.ReadAsByteArrayAsync();
                    return true;
                }
                else
                {
                    task = null;
                    return false;
                }
            }
        }

        public static async Task<byte[]> GetContentFromUriAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (uri.IsFile)
            {
                return await System.IO.File.ReadAllBytesAsync(uri.AbsolutePath, cancellationToken);
            }
            else
            {
                HttpClient client = new HttpClient();
                ContentDownloader d = new ContentDownloader(client);
                return await d.GetFromUriAsync(uri, cancellationToken);
            }
        }
    }
}
