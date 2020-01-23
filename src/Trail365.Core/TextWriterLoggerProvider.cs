using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Trail365
{
    public class TextWriterLoggerProvider : ILoggerProvider
    {
        private readonly TextWriter Writer = null;

        public TextWriterLoggerProvider(TextWriter writer)
        {
            this.Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public ILogger CreateLogger(string categoryName)

            => new EasyLogger(this.Writer.WriteLine, categoryName);

        public void Dispose()
        {
            //nothing to dispose here, caller should Dispose the Textwriter!
        }
    }
}
