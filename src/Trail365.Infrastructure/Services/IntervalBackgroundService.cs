using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trail365.Internal;

namespace Trail365.Services
{
    public abstract class IntervalBackgroundService : BackgroundServiceBase, IHostedService, IDisposable
    {
        private bool _isRunning = false;
        private Timer _timer = null;

        public bool CancellationOnInterval { get; set; } = true;

        /// <summary>
        /// wie lange soll der Timer vor dem ersten start warten, default 10 sekunden
        /// </summary>
        public TimeSpan DueTime { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Timer Interval, default 90 Sekunden.
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(90);

        public IntervalBackgroundService(IServiceProvider services, ILogger logger) : base(services, logger)
        {
        }

        protected abstract Task ExecuteAsync(CancellationToken token);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogDebug($"Service {this.GetType().Name} is starting.");
            //lt. specs wird der callback mehrfach parallel gestartet, wenn die duration länger als das interval ist!
            object lockObject = new object();
            _timer = new Timer(this.CallbackWrapper, lockObject, this.DueTime, this.Interval);
            return Task.CompletedTask;
        }

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        /// <summary>
        /// vermeide mehrfacheintritte, verursacht durch System.Threading.Timer und erlaube Async pattern in der Ausführenden Methode!
        /// </summary>
        /// <param name="state"></param>
        private void CallbackWrapper(object state)
        {
            Guard.Assert(state != null);
            lock (state)
            {
                if (_isRunning) return;
                _isRunning = true;
            }
            try
            {
                CancellationTokenSource ctSource = _stoppingCts;
                if (this.CancellationOnInterval)
                {
                    TimeSpan maxExecutionTime = this.Interval.Subtract(TimeSpan.FromSeconds(1));
                    CancellationTokenSource timeout = new CancellationTokenSource(maxExecutionTime);
                    ctSource = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, _stoppingCts.Token);
                }

                Stopwatch duration = Stopwatch.StartNew();
                try
                {
                    this.ExecuteAsync(ctSource.Token).GetAwaiter().GetResult();
                }
                catch (Exception exception)
                {
                    this.Logger.LogError(exception, $"{nameof(this.ExecuteAsync)}, IsCancellationRequested={ctSource.Token.IsCancellationRequested}");
                }
                finally
                {
                    duration.Stop();
                    this.Logger.LogTrace($"{this.GetType().Name}.{nameof(this.ExecuteAsync)} stopped. Duration={duration.ElapsedMilliseconds} msec");
                }
            }
            finally
            {
                lock (state)
                {
                    _isRunning = false;
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _stoppingCts.Dispose();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogDebug("Service is stopping.");
            _stoppingCts.Cancel();
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
