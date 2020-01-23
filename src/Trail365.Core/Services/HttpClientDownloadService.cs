using System.Net.Http;
using System.Threading.Tasks;

namespace Trail365.Services
{
    public class HttpClientDownloadService : DownloadService
    {
        public static DownloadService DefaultInstance = new HttpClientDownloadService();

        public HttpClientDownloadService()
        {
        }

        public override Task<byte[]> GetByteArrayAsync(string url)
        {
            byte[] result = null;
            using (var client = new HttpClient())
            {
                result = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            return Task.FromResult(result);
        }
    }
}
