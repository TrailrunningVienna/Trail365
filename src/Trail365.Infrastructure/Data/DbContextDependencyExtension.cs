using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Trail365.Internal;

namespace Trail365.Data
{
    public static class DbContextDependencyExtension
    {
        internal static bool TryGetTelemetryClient(this DbContext context, out TelemetryClient client)
        {
            IInfrastructure<IServiceProvider> isp = context ?? throw new ArgumentNullException(nameof(context));

            try
            {
                client = isp.GetService<TelemetryClient>();
            }
            catch (InvalidOperationException)
            {
                client = null;
                return false;
            }

            return (client != null);
        }

        internal static DependencyTelemetryTracker CreateDependencyTracker(this DbContext context, string operationTarget, string operationName, string operationType)
        {
            Guard.AssertNotNull(context);
            Guard.AssertNotNullOrEmptyString(operationTarget);
            Guard.AssertNotNullOrEmptyString(operationName);
            Guard.AssertNotNullOrEmptyString(operationType);

            if (TryGetTelemetryClient(context, out var client))
            {
                var operation = client.StartOperation<DependencyTelemetry>(operationName);
                operation.Telemetry.Target = operationTarget;
                operation.Telemetry.Type = operationType;
                return new DependencyTelemetryTracker(operation.Telemetry, operation);
            }
            else
            {
                var dt = new DependencyTelemetry();
                dt.Target = operationTarget;
                dt.Type = operationType;
                return new DependencyTelemetryTracker(dt, null);
            }
        }
    }
}
