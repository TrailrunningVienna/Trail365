using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Seeds;
using Trail365.Services;
using Trail365.Tasks;

namespace Trail365.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// This Method is called BEFORE the method 'Configure(IAppBuilder....')
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(this.Configuration);
            var settings = new AppSettings();
            this.Configuration.Bind(settings);

            if (settings.HasInstrumentationKey())
            {
                var aiStorageFolder = settings.ApplicationInsightsStorageFolder;
                if (!string.IsNullOrEmpty(aiStorageFolder))
                {
                    // For Linux OS
                    services.AddSingleton<ITelemetryChannel>(new ServerTelemetryChannel { StorageFolder = aiStorageFolder });
                }

                services.AddApplicationInsightsTelemetry((options) =>
               {
                   options.DeveloperMode = true;
               });
            }

            //https://docs.microsoft.com/en-us/azure/app-service/containers/configure-language-dotnetcore

            if (settings.EnableForwardHeaders)
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    // Only loopback proxies are allowed by default.
                    // Clear that restriction because forwarders are enabled by explicit
                    // configuration.
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:10.0.0.0"), 104));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:192.168.0.0"), 112));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:172.16.0.0"), 108));
                });
            }

            services.AddMvc();

            services.AddAuthorization();

            services.AddControllers();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => !context.Request.Host.ToString().ToLowerInvariant().Contains("localhost");
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.Configure<RequestLocalizationOptions>(rlo =>
            {
                var supportedCultures = new[]
                {
                  new CultureInfo("de-AT"),
                  new CultureInfo("de-DE"),
                  new CultureInfo("de"),
                  new CultureInfo("en"),
                };
                rlo.SupportedCultures = supportedCultures;
                rlo.SupportedUICultures = supportedCultures;
                rlo.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("de");
            });

            services.ConfigureDatabase(settings);

            services.ConfigureAuthentication(settings);

            services.ConfigureHealthApi(this.Configuration);

            services.ConfigureServices(this.Configuration);

            services.AddResponseCompression(opt =>
            {
                opt.EnableForHttps = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Auth/SignIn";
                options.AccessDeniedPath = "/Auth/SignIn";
                options.SlidingExpiration = true;
            });

            if (!settings.BackgroundServiceDisabled)
            {
                services.AddHostedService<QueuedHostedService>();
            }

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="result"></param>
        /// <param name="settings">in case of Environment==Development we print out some settings</param>
        /// <returns></returns>
        private static Task DefaultApiInfoHealthResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            var json = new JObject(
                new JProperty("Status", result.Status.ToString()),
                new JProperty("Version", Helper.GetProductVersionFromEntryAssembly()),
                new JProperty("ProcessUpTime", Helper.GetUptime()),
                new JProperty("ProcessStartTime", Helper.GetStartTime()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(p => new JProperty(p.Key, p.Value))))))))));
            return httpContext.Response.WriteAsync(json.ToString(Formatting.Indented));
        }

        public static void ConfigureRobotsTxt(IWebHostEnvironment environment, bool allowRobots)
        {
            //robots.txt wird als statisches file im wwwroot bereitgestellt.
            //hier können wir den passenden Inhalt generieren (speziell für Einschränkungen) oder das File auch löschen => keine EInschränkungen!
            if (string.IsNullOrEmpty(environment.WebRootPath))
            {
                //WM 08.07.2019 tritt in xUnit auf. kann dort ignoriert werden!
                return;
            }
            //Azure Apps for Container uses a special txt as health check.

            //robots933456.txt
            string azureFilePath = System.IO.Path.Combine(environment.WebRootPath, "robots933456.txt");

            if (!File.Exists(azureFilePath))
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("User-agent: *");
                System.IO.File.WriteAllText(azureFilePath, sb.ToString());
            }

            string filePath = System.IO.Path.Combine(environment.WebRootPath, "robots.txt");
            if (allowRobots)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); //meine: keine Restriktionen für Suchmaschinen!
                }
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("User-agent: *");
                sb.AppendLine("Disallow: /");
                System.IO.File.WriteAllText(filePath, sb.ToString());
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// This method is called AFTER 'ConfigureServices'
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="lifetime"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            AppSettings settings = app.ApplicationServices.GetRequiredService<IOptions<AppSettings>>().Value;

            if (settings.EnableForwardHeaders)
            {
                app.UseForwardedHeaders();
            }

            app.UseResponseCompression();

            //may be a bug in aspnetcore: rlo MUST be available via GetService => working sample here: https://github.com/aspnet/Entropy/blob/master/samples/Localization.StarterWeb/Startup.cs
            var rloOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value;

            app.UseRequestLocalization(rloOptions);

            lifetime.ApplicationStarted.Register(() =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation(ApplicationEventId.ApplicationStarted, $"Application started");
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation(ApplicationEventId.ApplicationStopping, $"Application stop requested");

                if (app.ApplicationServices.GetService(typeof(TelemetryClient)) is TelemetryClient client)
                {
                    client.Flush();
                }
            });

            lifetime.ApplicationStopped.Register(() =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation(ApplicationEventId.ApplicationStopped, $"Application stopped");

                if (app.ApplicationServices.GetService(typeof(TelemetryClient)) is TelemetryClient client)
                {
                    client.Flush();
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/home");
                //app.UseHsts();// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            var console = System.IO.TextWriter.Null;// System.Console.Out;

            if (env.IsDevelopment())
            {
                console = System.Console.Out;
            }
            //in Docker we must assume that every app start means a fresh container without data
            //each DBFile must be restored from the persistent storage location
            if (settings.SyncEnabled)
            {
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
            }

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

            if (settings.RunMigrationsAtStartup)
            {
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
            else
            {
                //special case: don't migrate on start but create database if does not exists currently NOT implemented!
            }

            if (settings.StaticUserSettingsEnabled)
            {
                if (settings.StaticUserSettings == null)
                {
                    throw new InvalidOperationException("StaticUserSettings missing in Appsettings");
                }
                app.UseMiddleware<AuthenticatedRequestMiddleware>();
            }
            else
            {
                app.UseAuthentication();
            }

            ConfigureRobotsTxt(env, settings.AllowRobots);

            int maxeAgeInSecondsForStaticAssets = 1 * 60; //1min

            if (env.IsProduction())
            {
                maxeAgeInSecondsForStaticAssets = maxeAgeInSecondsForStaticAssets * 60 * 24;
            }

            //wwwroot static file delivery
            FileExtensionContentTypeProvider contentType = new FileExtensionContentTypeProvider();
            contentType.Mappings[".webmanifest"] = "application/manifest+json";
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentType,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
                        "public,max-age=" + maxeAgeInSecondsForStaticAssets;
                }
            });

            if (!string.IsNullOrEmpty(settings.FileSystemBlobServiceRootDirectory))
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
                blobTypes.Mappings[".gpx"] = "application/gpx+xml";

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

            app.UseRouting();

            //app.UseCookiePolicy(); //WM 12/2019 commented out for https://github.com/aspnet/Security/issues/1755#issuecomment-431622343

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                // This custom writer formats the detailed status as JSON.
                ResponseWriter = DefaultApiInfoHealthResponse
            });

            app.UseAuthorization(); //The call to app.UseAuthorization() must appear between app.UseRouting() and app.UseEndpoints(...).

            app.UseEndpoints(endpoints =>
            {
                //https://stackoverflow.com/questions/58352836/how-to-define-an-endpoint-route-to-multiple-areas
                //https://github.com/aspnet/AspNetCore/blob/master/src/MusicStore/samples/MusicStore/Startup.cs
                //https://aregcode.com/blog/2019/dotnetcore-understanding-aspnet-endpoint-routing/

                //endpoints.MapControllerRoute(
                //    name: "Default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");

                //endpoints.MapAreaControllerRoute(
                //    name: "Backend",
                //    areaName: "Backend",
                //    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                  name: "areaRoute",
                  pattern: "{area:exists}/{controller}/{action}/{id?}",
                  defaults: new { action = "Index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
