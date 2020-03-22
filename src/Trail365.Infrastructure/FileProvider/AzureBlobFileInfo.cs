using System;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace Trail365.FileProvider
{
    internal class AzureBlobFileInfo : IFileInfo
    {
        private readonly CloudBlockBlob _blockBlob;

        public AzureBlobFileInfo(IListBlobItem blob)
        {
            switch (blob)
            {
                case CloudBlobDirectory d:
                    this.Exists = true;
                    this.IsDirectory = true;
                    this.Name = ((CloudBlobDirectory)blob).Prefix.TrimEnd('/');
                    //this.PhysicalPath = d.StorageUri.PrimaryUri.ToString();
                    break;

                case CloudBlockBlob b:
                    _blockBlob = b;
                    this.Name = !string.IsNullOrEmpty(b.Parent.Prefix) ? b.Name.Replace(b.Parent.Prefix, "") : b.Name;
                    this.Exists = b.Exists(); //TODO avoid this roundtrip !?
                    if (this.Exists)
                    {
                        b.FetchAttributes();
                        this.Length = b.Properties.Length;
                        //PhysicalPath = b.Uri.ToString();
                        this.LastModified = b.Properties.LastModified ?? DateTimeOffset.MinValue;
                    }
                    else
                    {
                        this.Length = -1;
                        // IFileInfo.PhysicalPath docs say: Return null if the file is not directly accessible.
                        // (PhysicalPath should maybe also be null for blobs that do exist but that would be a potentially breaking change.)
                        //PhysicalPath = null;
                    }
                    break;
            }
        }

        public Stream CreateReadStream()
        {
            var stream = new MemoryStream();
            _blockBlob.DownloadToStream(stream);
            stream.Position = 0;
            return stream;
        }

        public bool Exists { get; }
        public long Length { get; }
        public string PhysicalPath { get; }
        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public bool IsDirectory { get; }
    }
}

