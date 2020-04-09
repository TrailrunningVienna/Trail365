using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.FileProvider;
using Trail365.Internal;
using Trail365.Seeds;
using Trail365.Services;

namespace Trail365.Web
{
    public static class ApplicationBuilderExtension
    {

        public static void UseWwwRootStaticFileDelivery(this IApplicationBuilder app, AppSettings settings)
        {
            //wwwroot static file delivery
            FileExtensionContentTypeProvider contentType = new FileExtensionContentTypeProvider();
            contentType.Mappings[".webmanifest"] = "application/manifest+json";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentType,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
                        "public,max-age=" + settings.MaxAgeInSecondsForStaticAssets.ToString();
                }
            });
        }

        public static void UseMigrations(this IApplicationBuilder app, AppSettings settings)
        {
            Guard.Assert(settings.RunMigrationsAtStartup);
            //Lesson learned 12/2019: a database created via "EnsureCreated" cannot be migrated anymore - first creation must also be done by ".Migrate"!!!
            if (!settings.TrailContextDisabled)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetService<TrailContext>();
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
                    var dcr = (RelationalDatabaseCreator)context.GetService<IDatabaseCreator>();
                    bool exists = dcr.Exists();

                    context.Database.Migrate();

                    if ((settings.SeedOnCreation) && (!exists))
                    {
                        var blobService = context.GetService<BlobService>();
                        IUrlHelper helper = UrlHelperFactory.GetStaticUrlHelper(new UriBuilder(settings.SeedingApplicationUrl).Uri);
                        context.SeedTrails(TrailDtoProvider.CreateInstanceForPublicSeeds(), blobService, helper);
                        context.SeedEvents(EventDtoProvider.CreateTRVEvents2020(), blobService, helper);
                        context.SeedStories(StoryDtoProvider.RealisticStories(), blobService, helper);
                    }
                }
            }

            if (!settings.IdentityContextDisabled)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetService<IdentityContext>();
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
                    var dcr = (RelationalDatabaseCreator)context.GetService<IDatabaseCreator>();
                    bool exists = dcr.Exists();

                    context.Database.Migrate();

                    if ((settings.SeedOnCreation) && (!exists))
                    {
                    }
                }
            }

            if (!settings.TaskContextDisabled)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetService<TaskContext>();
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
                    var dcr = (RelationalDatabaseCreator)context.GetService<IDatabaseCreator>();
                    bool exists = dcr.Exists();

                    context.Database.Migrate();

                    if ((settings.SeedOnCreation) && (!exists))
                    {
                    }
                }
            }

        }

        public static void EnsureSqliteDefaultDirectories(this IApplicationBuilder app, AppSettings settings, TextWriter console)
        {
            if (ConnectionStrings.TryGetSqliteFileInfo(settings.ConnectionStrings.GetResolvedTrailDBConnectionString(), console, out var file1))
            {
                if (file1.Directory.Exists == false)
                {
                    file1.Directory.Create();
                }
            }

            if (ConnectionStrings.TryGetSqliteFileInfo(settings.ConnectionStrings.GetResolvedIdentityDBConnectionString(), console, out var file2))
            {
                if (file2.Directory.Exists == false)
                {
                    file2.Directory.Create();
                }
            }

            if (ConnectionStrings.TryGetSqliteFileInfo(settings.ConnectionStrings.GetResolvedTaskDBConnectionString(), console, out var file3))
            {
                if (file3.Directory.Exists == false)
                {
                    file3.Directory.Create();
                }
            }
        }

        public static IApplicationBuilder UseSqliteBackupSync(this IApplicationBuilder app, AppSettings settings, TextWriter console)
        {
            Guard.Assert(settings.SyncEnabled);

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                List<DbContext> list = new List<DbContext>();

                if (!settings.IdentityContextDisabled)
                {
                    list.Add(serviceScope.ServiceProvider.GetService<IdentityContext>());
                }

                if (!settings.TaskContextDisabled)
                {
                    list.Add(serviceScope.ServiceProvider.GetService<TaskContext>());
                }

                if (!settings.TrailContextDisabled)
                {
                    list.Add(serviceScope.ServiceProvider.GetService<TrailContext>());
                }
                DbContextExtension.SyncSqliteFiles(list.ToArray(), settings.BackupDirectory, settings.SyncOverwriteEnabled, console);
            }
            return app;
        }

        /// <summary>
        /// we cannot deliver gpx (download) via azure blob anymore => CORS is blocking href/download htlm5 features
        /// we implement a static file delivery by the app that acts like a proxy
        /// </summary>
        /// <param name="app"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static IApplicationBuilder UseStaticFileDeliveryForGpx(this IApplicationBuilder app, AppSettings settings)
        {
            string dir = Path.Combine(Path.GetTempPath(), "gpxProxyPath");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            FileExtensionContentTypeProvider blobTypes = new FileExtensionContentTypeProvider();
            blobTypes.Mappings[".gpx"] = "application/gpx+xml";

            var gpxBlobOptions = new StaticFileOptions()
            {
                ContentTypeProvider = blobTypes,
                FileProvider = new AzureBlobFileProvider(settings),
                RequestPath = new PathString($"/{RouteName.StorageProxyRoute}"), // settings.FileSystemBlobServiceRequestPath),
                ServeUnknownFileTypes = false,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
                        "public,max-age=" + settings.CloudStorageMaxAgeSeconds;
                }
            };

            app.UseStaticFiles(gpxBlobOptions);

            return app;
        }
        private static bool IsOriginAllowed(CorsPolicy policy, StringValues origin)
        {
            if (StringValues.IsNullOrEmpty(origin))
            {

                return false;
            }


            if (policy.AllowAnyOrigin || policy.IsOriginAllowed(origin))
            {

                return true;
            }

            return false;
        }

        public static IApplicationBuilder UseBlobServices(this IApplicationBuilder app, AppSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.FileSystemBlobServiceRootDirectory))
            {
                if (settings.CloudStorageEnabled)
                {
                    throw new InvalidOperationException("we shouldn't activate a static file route if cloud is used");
                }
                string dir = settings.GetFileSystemBlobServiceRootDirectoryResolved();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                //second static file area for file based blob storage

                FileExtensionContentTypeProvider blobTypes = new FileExtensionContentTypeProvider();
                blobTypes.Mappings[".gpx"] = SupportedMimeType.Gpx;
                blobTypes.Mappings[".geojson"] = SupportedMimeType.Geojson;
                var staticblobOptions = new StaticFileOptions()
                {
                    ContentTypeProvider = blobTypes,
                    FileProvider = new PhysicalFileProvider(dir),
                    RequestPath = new PathString(settings.FileSystemBlobServiceRequestPath),
                    ServeUnknownFileTypes = false,
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
                            "public,max-age=" + settings.CloudStorageMaxAgeSeconds;

                        var policy = app.ApplicationServices.GetRequiredService<CorsPolicy>();
                        if (ctx.File.Name.ToLowerInvariant().EndsWith(".geojson"))
                        {
                            var origin = ctx.Context.Request.Headers[CorsConstants.Origin];
                            var requestHeaders = ctx.Context.Request.Headers;

                            var isOptionsRequest = string.Equals(ctx.Context.Request.Method, CorsConstants.PreflightHttpMethod, StringComparison.OrdinalIgnoreCase);
                            var isPreflightRequest = isOptionsRequest && requestHeaders.ContainsKey(CorsConstants.AccessControlRequestMethod);

                            var corsResult = new CorsResult
                            {
                                IsPreflightRequest = isPreflightRequest,
                                IsOriginAllowed = IsOriginAllowed(policy, origin),
                            };

                            if (!corsResult.IsOriginAllowed)
                            {
                                ctx.Context.Response.StatusCode = 204;
                            }
                        }
                    }
                };

                app.UseStaticFiles(staticblobOptions);

                if (settings.FileSystemBlobServiceBrowserEnabled)
                {
                    app.UseDirectoryBrowser(new DirectoryBrowserOptions
                    {
                        FileProvider = new PhysicalFileProvider(dir),
                        RequestPath = new PathString(settings.FileSystemBlobServiceRequestPath),
                    });
                }
            }
            else
            {
                if (settings.CloudStorageEnabled)
                {
                    app.UseStaticFileDeliveryForGpx(settings); //proxy over blobstorage
                }
            }
            return app;
        }
    }
}
