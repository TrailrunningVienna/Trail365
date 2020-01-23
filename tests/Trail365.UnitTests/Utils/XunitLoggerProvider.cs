using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Utils
{
    // https://stackoverflow.com/questions/46169169/net-core-2-0-configurelogging-xunit-test
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }

        public ILogger CreateLogger(string categoryName)

            => new EasyLogger(_testOutputHelper.WriteLine, categoryName);

        public void Dispose()
        { }
    }
}
