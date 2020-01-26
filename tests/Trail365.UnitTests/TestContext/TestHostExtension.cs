using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Services;
using Trail365.ViewModels;
using Trail365.Web;
using Trail365.Web.Controllers;
using api = Trail365.Web.Api.Controllers;
using Backend = Trail365.Web.Backend.Controllers;

namespace Trail365.UnitTests.TestContext
{
    public static class TestHostExtension
    {
        public static readonly string CloudStorageEnvironmentVariable = "%STORAGE_MSSDEV_BB%";

        public static FacebookEventImporter CreateFacebookEventImporter(this TestHost host, DownloadService downloadService)
        {
            return new FacebookEventImporter(host.TrailContext, host.RootUrl, downloadService, host.BlobService);
        }

        public static void InitWithViewModel(this TestHost host, IEnumerable<TrailViewModel> trails)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            if (trails == null) throw new ArgumentNullException(nameof(trails));
            var entities = trails.Select(tvm =>
            {
                var trail = tvm.ToTrail();
                trail.GpxBlob = new Blob();
                trail.GpxBlobID = trail.GpxBlob.ID;
                host.BlobService.UploadBytesAsGpx(tvm.Gpx, trail.GpxBlob, host.RootUrl);
                return trail;
            }).ToList();
            host.TrailContext.ImportTrails(entities);
        }

        /// <summary>
        /// API-Controller for api/trails
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static api.TrailsController CreateTrailsController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var logger = host.ServiceProvider.GetRequiredService<ILogger<api.TrailsController>>();

            var queue = host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            var serviceScopeFactory = host.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

            var tc = new api.TrailsController(host.TrailContext, host.BlobService, logger, host.ServiceProvider.GetRequiredService<TrailImporterService>(), queue, serviceScopeFactory)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return tc;
        }

        public static TrailPreviewController CreateTrailPreviewController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var queue = host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            var serviceScopeFactory = host.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            var tc = new TrailPreviewController(host.TrailContext, serviceScopeFactory, queue)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return tc;
        }

        public static api.ManagementController CreateManagementController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var queue = host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            var serviceScopeFactory = host.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            var logger = host.ServiceProvider.GetRequiredService<ILogger<api.ManagementController>>();
            var tc = new api.ManagementController(queue, serviceScopeFactory, logger)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };

            return tc;
        }

        public static api.EventsController CreateEventsController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var logger = host.ServiceProvider.GetRequiredService<ILogger<api.EventsController>>();
            var tc = new api.EventsController(host.TrailContext, host.BlobService, host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>(), logger)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return tc;
        }

        public static api.StoriesController CreateStoryApiController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var logger = host.ServiceProvider.GetRequiredService<ILogger<api.StoriesController>>();
            var tc = new api.StoriesController(host.ServiceProvider.GetRequiredService<TrailContext>(), host.ServiceProvider.GetRequiredService<BlobService>(), logger)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return tc;
        }

        public static ImageController CreateImageController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var logger = host.ServiceProvider.GetRequiredService<ILogger<ImageController>>();
            var ic = new ImageController(host.ServiceProvider.GetRequiredService<TrailContext>(), host.ServiceProvider.GetRequiredService<BlobService>(), logger)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return ic;
        }

        public static StoryDetailsController CreateStoryDetailsController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var context = host.ServiceProvider.GetRequiredService<TrailContext>();
            var hc = new StoryDetailsController(context);
            hc.ApplyTestHostSettings(host);
            return hc;
        }

      
        public static HomeController CreateHomeController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));

            var context = host.ServiceProvider.GetRequiredService<TrailContext>();
            var settingsMonitor = host.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>();
            var logger = host.ServiceProvider.GetRequiredService<ILogger<HomeController>>();
            var queue = host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            var serviceScopeFactory = host.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            var cache = host.ServiceProvider.GetRequiredService<IMemoryCache>();
            var hc = new HomeController(context, settingsMonitor, logger, queue, serviceScopeFactory, cache);
            hc.ApplyTestHostSettings(host);
            return hc;
        }

        public static Backend.TrailsController CreateBackendTrailsController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var context = host.ServiceProvider.GetRequiredService<TrailContext>();
            var settingsMonitor = host.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>();
            var blobService = host.ServiceProvider.GetRequiredService<BlobService>();
            var controller = new Backend.TrailsController(context, settingsMonitor, blobService)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return controller;
        }

        private static void ApplyTestHostSettings(this Controller controller, TestHost host)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            controller.Url = HelperExtensions.EmptyUrlHelper;

            var userSettings = host.Settings.StaticUserSettings;
            if (userSettings == null)
            {
                throw new InvalidOperationException("Static-UserSettings not defined");
            }
            if (userSettings.ShouldNotLogin == false)
            {
                var principal = AuthenticatedRequestMiddleware.CreateClaimsPrincipal(userSettings);

                var userID = ServiceCollectionExtension.EnsureUserAsync(principal, host.ServiceProvider, true, userSettings.UserID).GetAwaiter().GetResult();

                if (userID != userSettings.UserID)
                {
                    throw new InvalidOperationException("Problems during user creation");
                }

                IClaimsTransformation ct = new ClaimsTransformer(host.IdentityContext, host.SettingsMonitor, host.LoggerFactory.CreateLogger<ClaimsTransformer>());
                var transformed = ct.TransformAsync(principal).GetAwaiter().GetResult();
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = transformed
                    }
                };
            }
        }

        public static FrontendController CreateFrontendController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var controller = new FrontendController(host.TrailContext, host.IdentityContext, host.BlobService);
            controller.ApplyTestHostSettings(host);
            return controller;
        }

        public static TrailController CreateTrailController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var cache = host.ServiceProvider.GetRequiredService<IMemoryCache>();
            var settingsMonitor = host.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>();
            var queue = host.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            var serviceScopeFactory = host.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

            var controller = new TrailController(host.ServiceProvider.GetRequiredService<TrailContext>(), host.ServiceProvider.GetRequiredService<BlobService>(), host.ServiceProvider.GetRequiredService<ILogger<TrailController>>(), cache, settingsMonitor,queue,serviceScopeFactory)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return controller;
        }

        public static Backend.PlacesController CreateBackendPlacesController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var controller = new Backend.PlacesController(host.TrailContext, host.SettingsMonitor)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return controller;
        }

        public static Backend.TasksController CreateBackendTasksController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var controller = new Backend.TasksController(host.TaskContext, host.SettingsMonitor)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return controller;
        }

        public static StoryController CreateStoryController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var cache = host.ServiceProvider.GetRequiredService<IMemoryCache>();
            var controller = new StoryController(host.TrailContext, cache, host.SettingsMonitor)
            {
                Url = HelperExtensions.EmptyUrlHelper
            };
            return controller;
        }

        public static StoryEditorController CreateStoryEditorController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var controller = new StoryEditorController(host.TrailContext, host.BlobService);
            controller.ApplyTestHostSettings(host);
            return controller;
        }

        public static StoryUploadController CreateStoryUploadController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var blobService = host.ServiceProvider.GetRequiredService<BlobService>();
            var controller = new StoryUploadController(host.ServiceProvider.GetRequiredService<TrailContext>(), blobService);
            controller.ApplyTestHostSettings(host);
            return controller;
        }

        public static AuthController CreateAuthController(this TestHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            IAuthenticationService authService = host.ServiceProvider.GetRequiredService<IAuthenticationService>();
            return new AuthController(authService);
        }
    }
}
