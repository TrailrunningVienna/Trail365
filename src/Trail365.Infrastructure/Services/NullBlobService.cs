using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Trail365.Services
{
    public class NullBlobService : BlobService
    {
        public override bool IsNull => true;

        protected override Uri CreateUploadUrl(Guid imageGUID, string type, string fileExtension, TimeSpan ttl, IUrlHelper helper)
        {
            return new UriBuilder("https://nowhere.com/my/file.blob").Uri;
        }

        protected override Uri UploadToUrl(string uriResult, Stream source, long sourceLength, string contentType, IUrlHelper helper)
        {
            return new UriBuilder("https://nowhere.com/my/file.blob").Uri;
        }
    }
}
