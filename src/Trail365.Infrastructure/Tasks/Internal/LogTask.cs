using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Trail365.Tasks.Internal
{
    public class LogTask : BackgroundTask
    {
        private void LogAllLevels(ILogger target)
        {
            Exception sampleException = new InvalidOperationException("SampleException!");
            foreach (LogLevel ll in System.Enum.GetValues(typeof(LogLevel)))
            {
                if (ll == LogLevel.None) continue;
                if (ll == LogLevel.Error)
                {
                    target.Log(ll, 66, sampleException, $"my Message with Exception Level {ll}.");
                    continue;
                }

                if (ll == LogLevel.Trace)
                {
                    target.Log(ll, 99, $"my multiline message{Environment.NewLine} for Level {ll}{Environment.NewLine}with 3 rows.");
                    continue;
                }
                target.Log(ll, 77, $"my Message for Level {ll}.");
            }
        }

        protected override Task Execute(CancellationToken cancellationToken)
        {
            var customLogger = this.Context.LoggerFactory.CreateLogger("custom");
            this.LogAllLevels(customLogger);
            this.LogAllLevels(this.Context.DefaultLogger);
            return Task.CompletedTask;
        }
    }
}
