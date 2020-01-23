using System;
using Trail365.Entities;

namespace Trail365
{
    public class BlobMapping
    {
        public string FolderName { get; set; }

        public int? StorageSize { get; set; }
        public string ContentHash { get; set; }
        public string MimeType { get; set; }
        public Uri Url { get; set; }

        public int? ImageWidth { get; set; }

        public int? ImageHeight { get; set; }

        public static BlobMapping ReadFromBlob(Blob blob)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));
            return new BlobMapping
            {
                FolderName = blob.FolderName,
                StorageSize = blob.StorageSize,
                MimeType = blob.MimeType,
                Url = new UriBuilder(blob.Url).Uri,
                ContentHash = blob.ContentHash,
                ImageHeight = blob.ImageHeight,
            };
        }

        public bool ApplyToBlob(Blob blob)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            BlobMapping mapping = this;
            bool hasChanges = false;
            string proposedUrl = mapping.Url.ToString();
            if (blob.Url != proposedUrl)
            {
                blob.Url = proposedUrl;
                hasChanges = true;
            }
            if (blob.StorageSize != mapping.StorageSize)
            {
                blob.StorageSize = mapping.StorageSize;
                hasChanges = true;
            }
            if (blob.MimeType != mapping.MimeType)
            {
                blob.MimeType = mapping.MimeType;
                hasChanges = true;
            }
            if (blob.FolderName != mapping.FolderName)
            {
                blob.FolderName = mapping.FolderName;
                hasChanges = true;
            }

            if (blob.ImageWidth != mapping.ImageWidth)
            {
                blob.ImageWidth = mapping.ImageWidth;
                hasChanges = true;
            }

            if (blob.ImageHeight != mapping.ImageHeight)
            {
                blob.ImageHeight = mapping.ImageHeight;
                hasChanges = true;
            }
            if (blob.ContentHash != mapping.ContentHash)
            {
                blob.ContentHash = mapping.ContentHash;
                hasChanges = true;
            }
            return hasChanges;
        }
    }
}
