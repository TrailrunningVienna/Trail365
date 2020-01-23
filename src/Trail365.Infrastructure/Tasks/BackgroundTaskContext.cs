using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Trail365.Tasks
{
    public class BackgroundTaskContext
    {
        public IUrlHelper Url { get; internal set; }
        internal IServiceScopeFactory ServiceScopeFactory { get; set; }
        public IServiceProvider ServiceProvider { get; internal set; }

        /// <summary>
        /// provides logging into TaskLog (DB)
        /// </summary>
        public ILogger DefaultLogger { get; internal set; }

        public ILoggerFactory LoggerFactory { get; internal set; }
    }
}
