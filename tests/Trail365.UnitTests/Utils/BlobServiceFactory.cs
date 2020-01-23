using System.IO;
using Trail365.Services;

namespace Trail365.UnitTests.Utils
{
    public static class BlobServiceFactory
    {
        private static readonly string RootFolder = Path.Combine(Path.GetTempPath(), "FSBlob");

        public static BlobService CreateLocalInstance()
        {
            string testFolder = Path.Combine(RootFolder, System.Guid.NewGuid().ToString("N"));
            var service = new FileSystemBlobService(testFolder, "/blob");
            return service;
        }
    }
}
