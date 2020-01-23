using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Trail365.Services
{
    public static class ServiceHelper
    {
        public static async Task<string> GetGpxXml(string url, ILogger logger = null)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var response = await httpClient.GetByteArrayAsync(url);
                logger.LogTrace($"{nameof(ServiceHelper)}.{nameof(GetGpxXml)}: {response.Length} bytes downloaded from '{url}'");
                return System.Text.Encoding.UTF8.GetString(response);
            }
        }
    }
}
