using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Utils
{
    public static class TestOutputHelperExtensions
    {

        public static ILogger CreateLogger(this ITestOutputHelper helper, string categoryName =null )
        {
            return CreateLoggerProvider(helper).CreateLogger(categoryName);
        }

        public static ILoggerProvider CreateLoggerProvider(this ITestOutputHelper helper)
        {
            return (helper != null) ? new XunitLoggerProvider(helper) : throw new ArgumentNullException(nameof(helper));
        }

    }
}
