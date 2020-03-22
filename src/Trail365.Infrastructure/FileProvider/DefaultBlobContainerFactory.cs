using System;
using Microsoft.Azure.Storage.Blob;
using Trail365.Configuration;
using Trail365.Services;

namespace Trail365.FileProvider
{
    internal class DefaultBlobContainerFactory : IBlobContainerFactory
    {
        private readonly CloudBlobContainer _container;

        public DefaultBlobContainerFactory(AppSettings settings)
        {
            if (settings.CloudStorageEnabled == false) throw new InvalidOperationException("CloudService not enabled, AzureBlobService should not be added to DI");
            string expandedConnectionString = settings.ConnectionStrings.GetResolvedCloudStorageConnectionString();
            string containerName = Environment.ExpandEnvironmentVariables(string.Format("{0}", settings.CloudStorageRootContainerName));

            CloudBlobClient blobClient = null;
            if (AzureBlobService.TryCreateBlobClient(expandedConnectionString, out var cl))
            {
                blobClient = cl;
                _container = cl.GetContainerReference(containerName);
                _container.CreateIfNotExists();
            }
            else
            {
                throw new InvalidOperationException("CloudStorageConnectionstring not valid/empty");
            }
        }

        public CloudBlobContainer GetContainer(string subpath)
        {
            return _container;
        }

        public string TransformPath(string subpath)
        {
            return subpath.TrimStart('/').TrimEnd('/');
        }
    }
}
