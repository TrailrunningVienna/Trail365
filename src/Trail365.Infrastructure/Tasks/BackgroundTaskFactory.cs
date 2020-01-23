using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Trail365.Tasks
{
    public static class BackgroundTaskFactory
    {
        public static T CreateTask<T>(IServiceScopeFactory serviceScopeFactory, IUrlHelper urlHelper, ILogger logger = null) where T : BackgroundTask, new()
        {
            var t = new T();
            BackgroundTaskContext cntx = new BackgroundTaskContext
            {
                Url = UrlHelperFactory.GetStaticUrlHelper(urlHelper ?? throw new ArgumentNullException(nameof(urlHelper))),
                ServiceScopeFactory = serviceScopeFactory
            };
            t.Context = cntx;
            if (logger != null)
            {
                t.logger = logger;
            }
            return t;
        }
    }
}
