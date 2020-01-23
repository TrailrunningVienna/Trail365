using System;
using Microsoft.ApplicationInsights.DataContracts;

namespace Trail365.Data
{
    public class DependencyTelemetryTracker : IDisposable
    {
        public DependencyTelemetryTracker(DependencyTelemetry _telemetry, IDisposable childOrDefault)
        {
            this.Telemetry = _telemetry ?? throw new ArgumentNullException(nameof(_telemetry));
            this.Child = childOrDefault;
        }

        private IDisposable Child;

        public DependencyTelemetry Telemetry { get; private set; }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.Child != null)
                    {
                        this.Child.Dispose();
                        this.Child = null;
                    }
                    this.Telemetry = null;
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
