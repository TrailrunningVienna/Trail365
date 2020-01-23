using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trail365.Internal;

namespace Trail365.Services
{
    public abstract class BlobService
    {
        public int CacheMaxAgeSeconds { get; set; } = 60 * 60 * 24;

        public virtual bool IsNull => false;

        private static readonly char[] FolderTrimChars = new char[] { '/', (char)92 };

        private static readonly char[] InvalidExtensionCharacters = new char[] { '/', '\\', '.', ' ' };

        /// <summary>
        ///
        /// </summary>
        /// <param name="imageGUID"></param>
        /// <param name="subFolderName">a grouping element for the blobs, on azure implemented as subfolder</param>
        /// <param name="uploadFileName"></param>
        /// <param name="sizeSuffix"></param>
        /// <returns></returns>
        public static string CalculateFileName(Guid imageGUID, string subFolderName, string fileExtension, string sizeSuffix = null)
        {
            string folder = $"{subFolderName}".ToLowerInvariant().Trim(FolderTrimChars);
            string ext = $"{fileExtension}".ToLowerInvariant().Trim('.');
            if (string.IsNullOrEmpty(ext)) throw new ArgumentNullException(nameof(fileExtension), $"'{fileExtension}'");
            if (folder != folder.Trim()) throw new InvalidOperationException("Trim-Bug");
            if (string.IsNullOrEmpty(folder)) throw new ArgumentNullException(nameof(subFolderName), $"'{subFolderName}'");
            if (folder.Contains('.')) throw new InvalidOperationException($"{nameof(subFolderName)} should not contain '.' ");
            string fileNameWithoutExtension = string.Format("{1}{0}{2}{3}", '/', folder, imageGUID.ToString("N"), sizeSuffix);
            Guard.Assert(!ext.StartsWith("."));
            foreach (Char c in ext)
            {
                if (InvalidExtensionCharacters.Contains(c))
                {
                    throw new InvalidOperationException("fileExtension contains invalid character");
                }
            }
            return fileNameWithoutExtension + "." + ext;
        }

        private static readonly TimeSpan TTL = TimeSpan.FromMinutes(5);

        /// <summary>
        /// InsertOrUpdate Strategie => last wins!
        /// </summary>
        /// <param name="blobGuid"></param>
        /// <param name="subFolder"></param>
        /// <param name="uploadFileName"></param>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public Uri Upload(Guid blobGuid, string subFolder, string fileExtension, Stream source, long length, IUrlHelper helper)
        {
            if (string.IsNullOrEmpty(fileExtension)) throw new ArgumentNullException(nameof(fileExtension));
            Uri url = this.CreateUploadUrl(blobGuid, subFolder, fileExtension, TTL, helper);
            string contentType = SupportedMimeType.CalculateMimeTypeFromFileExtension(fileExtension);
            var downloadUri = this.UploadToUrl(url.ToString(), source, length, contentType, helper);
            return downloadUri;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="imageID"></param>
        /// <param name="subFolder"></param>
        /// <param name="url">relative order rest-url, wir MÜSSEN daraus die Dateierweiterung ableiten können</param>
        public virtual void Remove(Guid imageID, string subFolder, string url)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="uriResult"></param>
        /// <param name="source"></param>
        /// <param name="sourceLength"></param>
        /// <param name="contentType">MimeType</param>
        /// <param name="helper"></param>
        /// <returns></returns>
        protected abstract Uri UploadToUrl(string uriResult, Stream source, long sourceLength, string contentType, IUrlHelper helper);

        protected abstract Uri CreateUploadUrl(Guid imageGUID, string subFolder, string fileExtension, TimeSpan ttl, IUrlHelper helper);
    }
}
