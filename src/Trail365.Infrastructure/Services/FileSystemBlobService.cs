using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Trail365.Internal;

namespace Trail365.Services
{
    public class FileSystemBlobService : BlobService
    {
        private readonly DirectoryInfo RootDirectory;
        private readonly string RequestPath;

        public FileSystemBlobService(string directoryPath, string requestPath)
        {
            if (string.IsNullOrEmpty(requestPath)) throw new ArgumentNullException(nameof(requestPath));
            if (requestPath.StartsWith(@"/") == false)
            {
                throw new InvalidOperationException("requestpath must start with '/'");
            }

            string expandedDirectoryName = Environment.ExpandEnvironmentVariables(string.Format("{0}", directoryPath));
            RootDirectory = new DirectoryInfo(expandedDirectoryName);
            if (RootDirectory.Exists == false)
            {
                RootDirectory.Create();
                RootDirectory.Refresh();
            }
            RequestPath = $"{requestPath}".Trim();
            //this.CacheMaxAgeSeconds = settings.CloudStorageMaxAgeSeconds;
        }

        protected override Uri UploadToUrl(string uriResult, Stream source, long sourceLength, string contentType, IUrlHelper helper)
        {
            if (string.IsNullOrEmpty(uriResult)) throw new ArgumentNullException(nameof(uriResult));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            Guard.Assert(source.CanRead);
            Guard.Assert(source.Position == 0);
            Uri target = new UriBuilder(uriResult).Uri;
            Guard.Assert(target.IsFile);

            string filePath = target.AbsolutePath;
            string dir = Path.GetDirectoryName(filePath);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = File.Create(target.AbsolutePath))
            {
                source.CopyTo(stream);
                if (stream.Length != source.Length)
                {
                    //this occures if the size injected into IFormFile is different then the real Stream size!
                    throw new InvalidOperationException($"Stream.Copy results in different stream.length: source={source.Length}, destination={stream.Length}");
                }
            }
            return helper.BuildFileSystemUri(target, RootDirectory.FullName, RequestPath);
        }

        public override void Remove(Guid imageID, string subFolder, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            UriBuilder builder = new UriBuilder(url);
            string fileExtension = Path.GetExtension(builder.Path);
            string fileName = CalculateFileName(imageID, subFolder, fileExtension: fileExtension);
            string fullpath = Path.Combine(RootDirectory.FullName, fileName);
            File.Delete(fullpath);
        }

        protected override Uri CreateUploadUrl(Guid imageGUID, string subFolder, string fileExtension, TimeSpan ttl, IUrlHelper helper)
        {
            string fileName = CalculateFileName(imageGUID, subFolder, fileExtension);
            string fullpath = Path.Combine(RootDirectory.FullName, fileName);
            var uri = new System.Uri(fullpath, UriKind.Absolute);
            Guard.Assert(uri.IsFile);
            return uri;
        }
    }
}
