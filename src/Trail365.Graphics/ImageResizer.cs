using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Trail365.Graphics.Internal;

namespace Trail365.Graphics
{
    public static class ImageResizer
    {
        //public static bool TryGetImageFormat(string imageType, out ImageFormat imageFormat)
        //{
        //    string s = CalculateWebContentType($"dummy.{imageType}", out imageFormat);
        //    return (imageFormat != null);
        //}

        //public static string CalculateWebContentType(string fileName, out ImageFormat imageFormat)
        //{
        //    string contentType = "application";
        //    imageFormat = null;
        //    if (string.IsNullOrEmpty(fileName))
        //    {
        //        return contentType;
        //    }

        //    string ext = Path.GetExtension(fileName).ToLowerInvariant();
        //    if (ext == ".png")
        //    {
        //        imageFormat = ImageFormat.Png;
        //        contentType = "image/png";
        //    }

        //    if (ext == ".jpg")
        //    {
        //        contentType = "image/jpg";
        //        imageFormat = ImageFormat.Jpeg;
        //    }

        //    if (ext == ".jpeg")
        //    {
        //        contentType = "image/jpg";
        //        imageFormat = ImageFormat.Jpeg;
        //    }

        //    if (ext == ".gpx")
        //    {
        //        contentType = "application/gpx+xml";
        //        imageFormat = null;
        //    }

        //    if (ext == ".bmp")
        //    {
        //        contentType = "image/bmp";
        //        imageFormat = ImageFormat.Bmp;
        //    }

        //    if (ext == ".gif")
        //    {
        //        contentType = "image/gif";
        //        imageFormat = ImageFormat.Gif;
        //    }

        //    if (ext == ".txt") contentType = "text/plain";

        //    return contentType;
        //}

        //public static bool TryGetSize(System.IO.Stream input, ImageFormat imageFormat, out System.Drawing.Size size)
        //{
        //    if (input == null) throw new ArgumentNullException(nameof(input));
        //    if (imageFormat == null) throw new ArgumentNullException(nameof(imageFormat));
        //    size = System.Drawing.Size.Empty;
        //    try
        //    {
        //        using (System.Drawing.Bitmap objImage = new System.Drawing.Bitmap(input))
        //        {
        //            size = objImage.Size;
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public static byte[] ResizeToBytes(byte[] input, ImageFormat imageFormat, Size size)
        //{
        //    using (var ms = new MemoryStream(input))
        //    {
        //        return ResizeToBytes(ms, imageFormat, size);
        //    }
        //}

        //public static byte[] ResizeToBytes(System.IO.Stream input, ImageFormat imageFormat, Size size)
        //{
        //    using (var ms = ResizeToStream(input, imageFormat, size))
        //    {
        //        return ms.ToArray();
        //    }
        //}

        public static MemoryStream Resize(System.IO.Stream input, System.Drawing.Size size)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            using (var image = Image.Load(input, out var imageFormat))
            {
                var sixSize = new Size(size.Width, size.Height);
                Resize(image, sixSize);
                var result = new MemoryStream(); //NO using pattern because NOT disposed if returned to caller!
                image.Save(result, imageFormat);
                result.Seek(0, SeekOrigin.Begin);
                Guard.Assert(result.Position == 0);
                return result;
            }
        }

        public static byte[] Resize(byte[] input, System.Drawing.Size size)
        {
            using (var ms = new MemoryStream(input))
            {
                using (var memoryStream = Resize(ms, size))
                {
                    return memoryStream.ToArray();
                }
            }
        }

        public static void Resize(string sourceFile, string targetFile, System.Drawing.Size targetSize)
        {
            if (targetSize.IsEmpty)
            {
                throw new InvalidOperationException($"{nameof(targetSize)} can't be empty");
            }
            using (var stream = File.OpenRead(sourceFile))
            {
                using (var ms = Resize(stream, targetSize))
                {
                    File.WriteAllBytes(targetFile, ms.ToArray());
                }
            }
        }

        public static Image Resize(Image targetImage, Size destinationSize)//, ImageFormat imageFormat)
        {
            if (destinationSize.IsEmpty)
            {
                throw new InvalidOperationException("target Size can't be empty");
            }

            //WM 22.07.2019: die kurze Seite des Bildes soll auf die Kantenlänge unseres Quadrats angepasst werden, die lange Seite im Verhältnis.

            int sourceWidth = targetImage.Width;
            int sourceHeight = targetImage.Height;
            //int sourceX = 0;
            //int sourceY = 0;

            //int destX = 0;
            //int destY = 0;
            int destWidth = destinationSize.Width;
            int destHeight = destinationSize.Height;

            Guard.Assert(destinationSize.Width == destinationSize.Height, "quadrat erwartet");
            if (sourceWidth != sourceHeight) //input ist nicht quadratisch
            {
                double factor = (Convert.ToDouble(sourceWidth) / Convert.ToDouble(sourceHeight));

                if (sourceHeight < sourceWidth) //height  ist die kurze Seite
                {
                    destHeight = destinationSize.Height;
                    destWidth = Convert.ToInt32(destinationSize.Width * factor);
                }
                else //width ist die kurze Seite
                {
                    destWidth = destinationSize.Width;
                    destHeight = Convert.ToInt32(destinationSize.Height / factor);
                }
            }

            //image.Mutate(x => x
            //       .Resize(image.Width / 2, image.Height / 2)
            //       .Grayscale());

            //  image.Save("output/fb.png"); // Automatic encoder selected based on extension.

            targetImage.Mutate(x => x.Resize(destWidth, destHeight));

            //Bitmap destinationBitmap;
            //if (imageFormat == ImageFormat.Png)
            //    destinationBitmap = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);
            //else if (imageFormat == ImageFormat.Gif)
            //    destinationBitmap = new Bitmap(destWidth, destHeight); //PixelFormat.Format8bppIndexed should be the right value for a GIF, but will throw an error with some GIF images so it's not safe to specify.
            //else
            //    destinationBitmap = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);

            //For some reason the resolution properties will be 96, even when the source image is different, so this matching does not appear to be reliable.
            //bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
            //If you want to override the default 96dpi resolution do it here
            //bmPhoto.SetResolution(72, 72);

            //using (Graphics graphic = Graphics.FromImage(destinationBitmap))
            //{
            //    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    graphic.DrawImage(sourceBitmap,
            //        new Rectangle(destX, destY, destWidth, destHeight),
            //        new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            //        GraphicsUnit.Pixel);
            //}
            return targetImage;
        }
    }
}
