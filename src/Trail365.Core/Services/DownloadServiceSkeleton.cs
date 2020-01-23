using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Trail365.Services
{
    public class DownloadServiceSkeleton : DownloadService
    {
        private readonly Func<string, byte[]> getByteArrayDelegate;

        public DownloadServiceSkeleton(Func<string, byte[]> callBack)
        {
            getByteArrayDelegate = callBack ?? throw new ArgumentNullException(nameof(callBack));
        }

        public override Task<byte[]> GetByteArrayAsync(string url)
        {
            using (var client = new HttpClient())
            {
                byte[] data = getByteArrayDelegate(url);
                if (data == null)
                {
                    throw new InvalidOperationException("null not allowed");
                }
                return Task.FromResult(data);
            }
        }
    }
}
