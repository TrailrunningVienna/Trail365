using System.IO;

namespace Trail365
{
    public static class SupportedMimeType
    {
        public static string CalculateMimeTypeFromFileName(string fileName)
        {
            string contentType = "application";

            if (string.IsNullOrEmpty(fileName))
            {
                return contentType;
            }
            string ext = Path.GetExtension(fileName);
            return CalculateMimeTypeFromFileExtension(ext);
        }

        public static readonly string ImageJpg = "image/jpg";
        public static readonly string ImagePng = "image/png";
        public static readonly string ImageBmp = "image/bmp";
        public static readonly string ImageGif = "image/gif";
        public static readonly string Application = "application";

        public static readonly string Gpx = "application/gpx+xml";
        public static readonly string Geojson = "application/json";

        public static bool IsTypeWithImageSize(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType)) return false;

            if (mimeType == ImageJpg) return true;
            if (mimeType == ImagePng) return true;
            if (mimeType == ImageGif) return true;
            if (mimeType == ImageBmp) return true;
            return false;
        }

        public static string CalculateMimeTypeFromFileExtension(string fileExtension)
        {
            string contentType = Application;

            if (string.IsNullOrEmpty(fileExtension))
            {
                return contentType;
            }

            string ext = fileExtension.ToLowerInvariant().Trim('.');

            if (ext == "png")
            {
                contentType = SupportedMimeType.ImagePng;
            }

            if (ext == "geojson")
            {
                contentType = SupportedMimeType.Geojson; 
            }

            if (ext == "jpg")
            {
                contentType = SupportedMimeType.ImageJpg;
            }

            if (ext == "jpeg")
            {
                contentType = SupportedMimeType.ImageJpg;
            }

            if (ext == "gpx")
            {
                contentType = SupportedMimeType.Gpx;
            }

            if (ext == "bmp")
            {
                contentType = SupportedMimeType.ImageBmp;
            }

            if (ext == "gif")
            {
                contentType = SupportedMimeType.ImageGif;
            }

            if (ext == "txt")
            {
                contentType = "text/plain";
            }

            if (ext == "md")
            {
                contentType = "text/markdown";
            }

            return contentType;
        }
    }
}
