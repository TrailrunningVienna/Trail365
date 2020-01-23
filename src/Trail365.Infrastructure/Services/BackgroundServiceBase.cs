using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;

namespace Trail365.Services
{
    public abstract class BackgroundServiceBase
    {
        private readonly IOptionsMonitor<AppSettings> Monitor;
        protected AppSettings Settings => Monitor.CurrentValue;
        protected IServiceProvider Services { get; private set; }
        protected ILogger Logger { get; private set; }

        public BackgroundServiceBase(IServiceProvider services, ILogger logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Services = services ?? throw new ArgumentNullException(nameof(services));
            Monitor = this.Services.GetRequiredService<IOptionsMonitor<AppSettings>>();
        }
    }
}
