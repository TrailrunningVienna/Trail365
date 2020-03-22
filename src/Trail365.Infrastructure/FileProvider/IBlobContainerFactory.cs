using Microsoft.Azure.Storage.Blob;

namespace Trail365.FileProvider
{
    public interface IBlobContainerFactory
    {
        CloudBlobContainer GetContainer(string subpath);
        string TransformPath(string subpath);
    }
}
