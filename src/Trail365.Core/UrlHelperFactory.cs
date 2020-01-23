using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Trail365
{
    public static class UrlHelperFactory
    {
        public static bool TryGetStaticUrlHelper(string baseUri, out IUrlHelper helper)
        {
            helper = null;
            if (string.IsNullOrWhiteSpace(baseUri)) return false;
            if (Uri.TryCreate(baseUri, UriKind.RelativeOrAbsolute, out var uri))
            {
                helper = uri.GetStaticUrlHelper();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Urlhelpers from Controller are depending on Request/Context and cannot passed to Background.
        ///
        /// </summary>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static IUrlHelper GetStaticUrlHelper(this IUrlHelper baseUri)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));

            if (baseUri == HelperExtensions.EmptyUrlHelper)
            {
                return baseUri;
            }

            var request = baseUri.ActionContext.HttpContext.Request;

            HttpContext httpContext = new DefaultHttpContext()
            {
                Request =
                {
                    Scheme = request.Scheme,
                    Host = request.Host,
                    PathBase = request.PathBase,
                    Path = request.Path
                },
            };

            // var captured = host.Services.GetRequiredService<Capture>();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData { },
                ActionDescriptor = new ActionDescriptor(),
            };
            return new UrlHelper(actionContext);
        }

        public static IUrlHelper GetStaticUrlHelper(this Uri baseUri)
        {
            HttpContext httpContext = new DefaultHttpContext()
            {
                Request =
                {
                    Scheme = baseUri.Scheme,
                    Host = HostString.FromUriComponent(baseUri),
                    PathBase = PathString.FromUriComponent(baseUri),
                },
            };

            // var captured = host.Services.GetRequiredService<Capture>();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData { },
                ActionDescriptor = new ActionDescriptor(),
            };
            return new UrlHelper(actionContext);
        }
    }
}
