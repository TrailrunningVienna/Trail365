using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Trail365.Tasks
{
    public static class BackgroundTaskFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="urlHelper"></param>
        /// <param name="logger">fallback logger used by the task engine</param>
        /// <returns></returns>
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
                t.QueueLogger = logger;
            }

            return t;
        }
    }
}
