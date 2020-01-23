using System;
using System.Threading.Tasks;
using Trail365.DTOs;

namespace Trail365.Services
{
    public abstract class DownloadService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="image"></param>
        /// <param name="downloadService"></param>
        /// <param name="url"></param>
        /// <returns>BlobDto as result because only the dto has a byte[] property that can contains the data</returns>
        public BlobDto DownloadFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            UriBuilder b = new UriBuilder(url);
            var i = b.Path.LastIndexOf(".");
            if (i < 1)
            {
                throw new InvalidOperationException("file extension cannot be caluclated from Url");
            }
            string imageType = b.Path.Substring(i + 1).ToLowerInvariant();
            string folderName = Utils.GetValidFolderName(imageType);
            var image = new BlobDto();
            image.Data = this.GetByteArrayAsync(url).GetAwaiter().GetResult();
            image.SubFolder = imageType;
            image.MimeType = SupportedMimeType.CalculateMimeTypeFromFileExtension(imageType);
            image.OriginalFileName = b.Path;
            return image;
        }

        public abstract Task<byte[]> GetByteArrayAsync(string url);
    }
}
