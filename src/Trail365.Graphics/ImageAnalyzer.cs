using System;
using System.IO;
using SixLabors.ImageSharp;
using Trail365.Graphics.Internal;

namespace Trail365.Graphics
{
    public static class ImageAnalyzer
    {
        public static string GetMimeType(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return GetMimeType(stream);
            }
        }

        public static System.Drawing.Size GetSize(Stream stream)
        {
            return GetSize(stream, out var _);
        }

        public static System.Drawing.Size GetSize(Stream stream, out string mimeType)
        {
            Guard.Assert(stream.Position == 0);
            using (var i = Image.Load(stream, out var format))
            {
                var sixSize = i.Size();
                mimeType = format.DefaultMimeType;
                return new System.Drawing.Size(sixSize.Width, sixSize.Height);
            }
        }

        public static string GetMimeType(Stream stream)
        {
            using (var image = Image.Load(stream, out var format))
            {
                if (string.IsNullOrEmpty(format.DefaultMimeType))
                {
                    throw new InvalidOperationException("DefaultMimeType not available");
                }
                return format.DefaultMimeType;
            }
        }
    }
}
