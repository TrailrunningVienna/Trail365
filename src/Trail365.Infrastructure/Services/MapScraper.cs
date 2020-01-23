using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Trail365.Graphics;

namespace Trail365.Services
{
    public abstract class MapScraper
    {
        public MapScraper(ILogger logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public readonly ILogger Logger = NullLogger.Instance;
        public virtual bool IsNull => false;

        public abstract Task<byte[]> ScreenshotAsync(Uri requestUri, System.Drawing.Size size);

        public MemoryStream Resize(System.IO.Stream input, Size size)
        {
            return ImageResizer.Resize(input, size);
        }
    }
}
