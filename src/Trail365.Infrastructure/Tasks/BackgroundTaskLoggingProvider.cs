using System;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.Entities;

namespace Trail365.Tasks
{
    public class BackgroundTaskLoggingProvider : ILoggerProvider
    {
        private readonly TaskContext _context;
        private TaskLogItem _currentLogItem = null;
        private readonly object _lockItem = new object();
        private readonly bool _disabled = false;

        public BackgroundTaskLoggingProvider(TaskContext context) : this(context, false)
        {
        }

        public BackgroundTaskLoggingProvider(TaskContext context, bool disabled)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._disabled = disabled;
        }

        private void WriteLogLine(LogLevel level, string category, string s)
        {
            if (_disabled) return;
            if (s == null) s = string.Empty;
            string levelAsString = level.ToString();
            lock (_lockItem)
            {
                if (_currentLogItem != null)
                {
                    if ((_currentLogItem.Level != levelAsString) || (_currentLogItem.Category != category))
                    {
                        _context.SaveChanges();
                        _currentLogItem = null;
                    }
                }

                if (_currentLogItem == null)
                {
                    _currentLogItem = new TaskLogItem
                    {
                        Level = levelAsString,
                        Category = category,
                    };
                    _context.TaskLogItems.Add(_currentLogItem);
                }

                if (string.IsNullOrEmpty(_currentLogItem.LogMessage))
                {
                    _currentLogItem.LogMessage = s;
                }
                else
                {
                    _currentLogItem.LogMessage += Environment.NewLine + s;
                }

                if ((_currentLogItem.LogMessage.Length > 1024 * 4) || DateTime.UtcNow.Subtract(_currentLogItem.TimestampUtc).TotalMilliseconds > 2000)
                {
                    _context.SaveChanges();
                    _currentLogItem = null;
                }
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new EasyLogger(this.WriteLogLine, categoryName);
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    lock (_lockItem)
                    {
                        if (_currentLogItem != null)
                        {
                            _context.SaveChanges();
                            _currentLogItem = null;
                        }
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
