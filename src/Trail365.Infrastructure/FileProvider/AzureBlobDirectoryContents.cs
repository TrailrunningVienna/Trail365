using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace Trail365.FileProvider
{
    internal class AzureBlobDirectoryContents : IDirectoryContents
    {
        private readonly List<IListBlobItem> _blobs = new List<IListBlobItem>();
        public bool Exists { get; set; }

        public AzureBlobDirectoryContents(CloudBlobDirectory blob)
        {
            BlobContinuationToken continuationToken = null;

            do
            {
                var response = blob.ListBlobsSegmented(continuationToken);
                continuationToken = response.ContinuationToken;
                _blobs.AddRange(response.Results);
            }
            while (continuationToken != null);

            this.Exists = _blobs.Count > 0;
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _blobs.Select(blob => new AzureBlobFileInfo(blob)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

