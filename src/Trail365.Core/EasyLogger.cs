using System;
using Microsoft.Extensions.Logging;

namespace Trail365
{
    public class EasyLogger : ILogger
    {
        private readonly Action<LogLevel, string, string> WritleLineDelegate = null;
        private readonly string _categoryName;
        public EasyLogger(Action<string> writeLineDelegate, string categoryName)
        {
            if (writeLineDelegate == null) throw new ArgumentNullException(nameof(writeLineDelegate));
            _categoryName = categoryName;
            this.WritleLineDelegate = (ll, cat, msg) =>
            {
                writeLineDelegate($"{_categoryName} ".TrimStart()+$"{msg}");
            };
        }

        public EasyLogger(Action<LogLevel, string, string> writeLineDelegate, string categoryName)
        {
            this.WritleLineDelegate = writeLineDelegate ?? throw new ArgumentNullException(nameof(writeLineDelegate));
            //if (string.IsNullOrWhiteSpace(categoryName))
            //{
            //    throw new ArgumentNullException(nameof(categoryName));
            //}
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
            => NoOpDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var stateString = formatter(state, exception);
            string exceptionStateString = string.Empty;

            if (exception != null)
            {
                exceptionStateString = exception.ToString();
            }
            WritleLineDelegate(logLevel, _categoryName, $"{stateString} {exceptionStateString}".TrimEnd());
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose()
            { }
        }
    }
}
