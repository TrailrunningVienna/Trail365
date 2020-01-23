using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Internal;

namespace Trail365.Services
{
    public class AzureBlobService : BlobService
    {
        //Zum Thema Wiederverwendung von Client und Container in verschiedenen Threads:
        //https://stackoverflow.com/questions/9934111/reuse-cloudblobclient-object

        private readonly CloudBlobClient _client;

        /// <summary>
        /// bezeichnet NICHT den root-container des gesamten Blob Storages, sondern den Container unterhalb dessen unsere images (Anzeigenbilder und avatare) angezeigt werden.
        /// </summary>
        private readonly CloudBlobContainer _rootContainer;

        public AzureBlobService(AppSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.CloudStorageEnabled == false) throw new InvalidOperationException("CloudService not enabled, AzureBlobService should not be added to DI");
            string expandedConnectionString = settings.ConnectionStrings.GetResolvedCloudStorageConnectionString();
            string containerName = Environment.ExpandEnvironmentVariables(string.Format("{0}", settings.CloudStorageRootContainerName));

            if (TryCreateBlobClient(expandedConnectionString, out var cl))
            {
                _client = cl;
                _rootContainer = _client.GetContainerReference(containerName);
                _rootContainer.CreateIfNotExists();
            }
            else
            {
                throw new InvalidOperationException("CloudStorageConnectionstring not valid/empty");
            }
            this.CacheMaxAgeSeconds = settings.CloudStorageMaxAgeSeconds;
        }

        public AzureBlobService(IOptionsMonitor<AppSettings> settings) : this(settings.CurrentValue)
        {
        }

        public static bool TryCreateBlobClient(string connectionString, out CloudBlobClient client)
        {
            client = null;
            if (string.IsNullOrEmpty(connectionString)) return false;
            if (CloudStorageAccount.TryParse(connectionString, out var cloudStorageAccount) == false)
            {
                return false;
            }
            client = cloudStorageAccount.CreateCloudBlobClient();
            return (client != null);
        }

        protected override Uri UploadToUrl(string uriResult, Stream source, long sourceLength, string contentType, IUrlHelper helper)
        {
            if (string.IsNullOrEmpty(uriResult)) throw new ArgumentNullException(nameof(uriResult));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Guard.Assert(source.CanRead);
            Uri url = new Uri(uriResult);
            var blob = new CloudBlockBlob(url);

            //https://alexandrebrisebois.wordpress.com/2013/08/11/save-money-by-setting-cache-control-on-windows-azure-blobs/

            if (this.CacheMaxAgeSeconds > 0)
            {
                var cacheControl = $"max-age={this.CacheMaxAgeSeconds}";
                cacheControl += " ,public";
                blob.Properties.CacheControl = cacheControl;
            }
            blob.Properties.ContentType = contentType;
            Guard.Assert(source.Position == 0);
            blob.UploadFromStream(source, sourceLength);
            return blob.StorageUri.PrimaryUri;
        }

        public override void Remove(Guid imageID, string subFolder, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            UriBuilder builder = new UriBuilder(url);
            string extension = Path.GetExtension(builder.Path);
            string fileName = CalculateFileName(imageID, subFolder, fileExtension: extension);
            var blob = _rootContainer.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="imageGUID"></param>
        /// <param name="ttl">UploadUrl has a TimeToLive (Timeout) because it is protected with a security token
        /// <returns></returns>
        protected override Uri CreateUploadUrl(Guid imageGUID, string subFolder, string fileExtension, TimeSpan ttl, IUrlHelper helper)
        {
            string fileName = CalculateFileName(imageGUID, subFolder, fileExtension);

            var blob = _rootContainer.GetBlockBlobReference(fileName);

            var uriBuilder = new UriBuilder(blob.Uri)
            {
                Query = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Write,
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.Add(ttl)
                }).Substring(1)
            };
            return uriBuilder.Uri; //Upload Url
        }
    }
}
