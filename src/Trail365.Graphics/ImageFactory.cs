using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Trail365.Graphics.Internal;

namespace Trail365.Graphics
{
    public static class ImageFactory
    {
        private static PointF[] SixConvert(System.Drawing.PointF[] points)
        {
            return points.Select(p => new PointF(p.X, p.Y)).ToArray();
        }

        public static byte[] CreateLineImageAsPng(System.Drawing.PointF[] points, System.Drawing.Size size)
        {
            using (Image i = CreateLineImage(points, size))
            {
                using (var ms = new MemoryStream())
                {
                    i.SaveAsPng(ms);
                    var result = ms.ToArray();
                    Guard.Assert(result.Length == ms.Length, "position shouldn't be relevant for ToArray");
                    return result;
                }
            }
        }

        public static Image CreateLineImage(System.Drawing.PointF[] points, System.Drawing.Size size)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (size.IsEmpty) throw new ArgumentException("Size cannot be empty");
            if (points.Length == 0)
            {
                throw new ArgumentException("points shoudn't be empty");
            }

            var image = new Image<Rgba32>(size.Width, size.Height);
            try
            {
                Pen pen = Pens.Solid(Color.Black, 1);

                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.Transparent);
                    imageContext.DrawLines(pen, SixConvert(points));
                });
            }
            catch (Exception)
            {
                image.Dispose();
                throw;
            }
            return image;
        }
    }
}
