using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Internal;
using Trail365.Services;

namespace Trail365.Web
{
    public class AppSettingsHealthCheck : IHealthCheck
    {

        Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            var healthCheckResultHealthy = false;

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("A healthy result."));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("An unhealthy result."));
        }
    }

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            AppSettings settings = new AppSettings();
            configuration.Bind(settings);

            if (settings.CloudStorageEnabled)
            {
                services.AddSingleton<BlobService>(isp =>
                {
                    IOptionsMonitor<AppSettings> settingsMonitor = isp.GetRequiredService<IOptionsMonitor<AppSettings>>();
                    return new AzureBlobService(settingsMonitor);
                });
            }
            else
            {
                if (string.IsNullOrEmpty(settings.FileSystemBlobServiceRootDirectory))
                {
                    services.AddSingleton<BlobService>(isp =>
                    {
                        return new NullBlobService();
                    });
                }
                else
                {
                    if (settings.FileSystemBlobServiceBrowserEnabled)
                    {
                        services.AddDirectoryBrowser();
                    }
                    services.AddSingleton<BlobService>(isp =>
                    {
                        return new FileSystemBlobService(settings.GetFileSystemBlobServiceRootDirectoryResolved(), settings.FileSystemBlobServiceRequestPath);
                    });
                }
            }

            if (settings.PuppeteerEnabled)
            {
                services.AddSingleton<MapScraper, PuppeteerScraper>();
            }
            else
            {
                services.AddSingleton<MapScraper, NullScraper>();
            }

            services.AddSingleton<ScrapingService>();

            services.AddSingleton<TrailImporterService>();

            return services;
        }

        public static IServiceCollection ConfigureHealthApi(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            IWebHostEnvironment thisEnv = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

            IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks();//.AddCheck("AppSettings", new AppSettingsHealthCheck());


            //healthChecksBuilder.AddCheck("AppSettings", () =>
            //{
            //    var isp = healthChecksBuilder.Services.BuildServiceProvider();
            //    AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
            //    IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

            //    Dictionary<string, object> dictionary = new Dictionary<string, object>
            //    {
            //       // { nameof(settings.BackgroundServiceDisabled), settings.BackgroundServiceDisabled.ToString() }
            //    };

            //    if (env.IsDevelopment())
            //    {
            //    }

            //    ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);

            //    HealthCheckResult r = new HealthCheckResult(HealthStatus.Degraded, description: string.Format("XXXX{0}", env.EnvironmentName), data: roDict);
            //    return r;
            //});


            healthChecksBuilder.AddCheck("TrailExplorer", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(settings.TrailExplorerBaseUrl), $"{settings.TrailExplorerBaseUrl}" }
                };

                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;

                if (env.IsDevelopment())
                {
                    if (settings.Features.TrailAnalyzer)
                    {
                        if (string.IsNullOrEmpty(settings.TrailExplorerBaseUrl))
                        {
                            proposedHealthStatatus = HealthStatus.Degraded;
                            proposedDescription = $"Feature '{nameof(settings.Features.TrailAnalyzer)}' is enabled but setting '{nameof(settings.TrailExplorerBaseUrl)}' is empty";
                        }
                    }

                    if (settings.SyncEnabled)
                    {
                        if (string.IsNullOrEmpty(settings.BackupDirectory))
                        {
                            proposedHealthStatatus = HealthStatus.Degraded;
                            proposedDescription = "BackupDirectory not defined but Sync is enabled!";
                        }
                    }

                }
                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });



            healthChecksBuilder.AddCheck("App", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(settings.BackgroundServiceDisabled), settings.BackgroundServiceDisabled.ToString() }
                };

                var proposedHealthStatatus = HealthStatus.Healthy;

                if (env.IsDevelopment())
                {
                    dictionary.Add(nameof(settings.HasInstrumentationKey), settings.HasInstrumentationKey());
                    dictionary.Add(nameof(settings.RunMigrationsAtStartup), settings.RunMigrationsAtStartup);
                    dictionary.Add(nameof(settings.PuppeteerEnabled), settings.PuppeteerEnabled);
                    //dictionary.Add(nameof(settings.TrailExplorerBaseUrl), $"{settings.TrailExplorerBaseUrl}");

                    dictionary.Add(nameof(settings.GoogleAuthentication), settings.GoogleAuthentication);
                    dictionary.Add(nameof(settings.FacebookAuthentication), settings.FacebookAuthentication);
                    dictionary.Add(nameof(settings.GitHubAuthentication), settings.GitHubAuthentication);

                    dictionary.Add(nameof(settings.AllowRobots), settings.AllowRobots);
                    dictionary.Add(nameof(settings.DisableImageDelivery), settings.DisableImageDelivery);
                    dictionary.Add(nameof(settings.MaxResultSize), settings.MaxResultSize);
                    dictionary.Add(nameof(settings.MaxPromotionSize), settings.MaxPromotionSize);

                    dictionary.Add(nameof(settings.SeedingApplicationUrl), settings.SeedingApplicationUrl);
                    dictionary.Add(nameof(settings.SeedOnCreation), settings.SeedOnCreation);

                    dictionary.Add($"{nameof(settings.FacebookSettings)}.{nameof(settings.FacebookSettings.ImporterDays)}", settings.FacebookSettings.ImporterDays);

                    dictionary.Add(nameof(settings.AbsoluteExpirationInSecondsRelativeToNow), settings.AbsoluteExpirationInSecondsRelativeToNow);

                    dictionary.Add(nameof(settings.BackupDirectory), settings.BackupDirectory);
                    dictionary.Add(nameof(settings.SyncEnabled), settings.SyncEnabled);
                    dictionary.Add(nameof(settings.SyncOverwriteEnabled), settings.SyncOverwriteEnabled);

                    dictionary.Add(nameof(settings.EnableForwardHeaders), settings.EnableForwardHeaders);
                    dictionary.Add(nameof(settings.ApplicationInsightsStorageFolder), settings.ApplicationInsightsStorageFolder);

                    dictionary.Add(nameof(settings.WEBSITES_ENABLE_APP_SERVICE_STORAGE), settings.WEBSITES_ENABLE_APP_SERVICE_STORAGE);

                    //Sqlite files should be without security sensite data (credentials)
                    dictionary.Add("TrailDBConnectionString", settings.ConnectionStrings.TrailDB);
                    dictionary.Add("ResolvedTrailDBConnectionString", settings.ConnectionStrings.GetResolvedTrailDBConnectionString());

                    dictionary.Add("IdentityDBConnectionString", settings.ConnectionStrings.IdentityDB);
                    dictionary.Add("ResolvedIdentityDBConnectionString", settings.ConnectionStrings.GetResolvedIdentityDBConnectionString());

                    dictionary.Add("TaskDBConnectionString", settings.ConnectionStrings.TaskDB);
                    dictionary.Add("ResolvedTaskDBConnectionString", settings.ConnectionStrings.GetResolvedTaskDBConnectionString());
                }
                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, description: string.Format("{0}", env.EnvironmentName), data: roDict);
                return r;
            });

            healthChecksBuilder.AddCheck("Blob", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(settings.CloudStorageEnabled), settings.CloudStorageEnabled }
                };

                if (env.IsDevelopment())
                {
                    string accountName = "N/A";

                    if (settings.CloudStorageEnabled)
                    {
                        if (CloudStorageAccount.TryParse(settings.ConnectionStrings.GetResolvedCloudStorageConnectionString(), out var account))
                        {
                            accountName = account.Credentials.AccountName;
                        }
                    }
                    dictionary.Add("CloudStorageRootContainerName", settings.CloudStorageRootContainerName);
                    dictionary.Add("CloudStorageMaxAgeSeconds", settings.CloudStorageMaxAgeSeconds);
                    dictionary.Add("CloudStorageAccount", accountName);
                    dictionary.Add(nameof(settings.FileSystemBlobServiceBrowserEnabled), settings.FileSystemBlobServiceBrowserEnabled);
                    dictionary.Add(nameof(settings.FileSystemBlobServiceRequestPath), settings.FileSystemBlobServiceRequestPath);
                    dictionary.Add(nameof(settings.FileSystemBlobServiceRootDirectory), settings.FileSystemBlobServiceRootDirectory);
                }

                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(HealthStatus.Healthy, description: "binary large objects settings", data: roDict);
                return r;
            });

            healthChecksBuilder.AddCheck("Globalization", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();
                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(CultureInfo.CurrentUICulture), CultureInfo.CurrentUICulture.DisplayName },
                    { nameof(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture.DisplayName },
                    { "TimeZoneInfo.Local.DisplayName", TimeZoneInfo.Local.DisplayName },
                    { "TimeZoneInfo.Local.BaseUtcOffset", TimeZoneInfo.Local.BaseUtcOffset },
                    { "TimeZoneInfo.Local.Id", TimeZoneInfo.Local.Id }
                };

                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(HealthStatus.Healthy, description: "globalization settings", data: roDict);
                return r;
            });

            healthChecksBuilder.AddCheck("Environment", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                if (env.IsDevelopment())
                {
                    //WM 02/2020 this is a high risk security breaker but sometimes needed if hosting has issues with the config binding!
                    //commented out, can be activated on demand!
                    //var sortedList = System.Environment.GetEnvironmentVariables().OfType<System.Collections.DictionaryEntry>().ToList().OrderBy(e => e.Key);
                    //foreach (System.Collections.DictionaryEntry de in sortedList)
                    //{
                    //    dictionary.Add($"{de.Key}", $"{de.Value}");
                    //}
                }
                else
                {
                    //Production Security... worst case some selected/not critical settings... no one is better!
                }
                ReadOnlyDictionary<string, object> data = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(HealthStatus.Healthy, description: "environment settings", data: data);
                return r;
            });

            healthChecksBuilder.AddCheck("Process", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();
                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                };

                dictionary.Add("Info", Helper.GetProcessInfo());
                System.Diagnostics.Process current = System.Diagnostics.Process.GetCurrentProcess();
                dictionary.Add("Threads", $"{current.Threads.Count}");
                dictionary.Add("WorkingSet64", $"{current.WorkingSet64}");
                dictionary.Add("VirtualMemorySize64", $"{current.VirtualMemorySize64}");
                dictionary.Add("PrivateMemorySize64", $"{current.PrivateMemorySize64}");

                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(HealthStatus.Healthy, description: "process properties", data: roDict);
                return r;
            });

            healthChecksBuilder.AddCheck("Deployment", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                string ld = "N/A";

                string deployFolder = Path.Combine(System.Environment.ExpandEnvironmentVariables("%HOME%"), "site/deployments");
                deployFolder = Path.GetFullPath(deployFolder);
                if (env.IsDevelopment())
                {
                    dictionary.Add("DeploymentDirectory", $"{deployFolder} (Exists={Directory.Exists(deployFolder)})");
                }
                string activeFile = Path.GetFullPath(Path.Combine(deployFolder, "active"));

                if (File.Exists(activeFile))
                {
                    string deploymentID = File.ReadAllText(activeFile).Trim();
                    if (env.IsDevelopment())
                    {
                        dictionary.Add("ActiveDeployment", deploymentID);
                    }
                    string statusFile = Path.GetFullPath(Path.Combine(deployFolder, $"{deploymentID}/status.xml"));
                    if (env.IsDevelopment())
                    {
                        dictionary.Add("DeploymentStatusFile", $"{statusFile} (Exists={File.Exists(statusFile)})");
                    }
                    if (File.Exists(statusFile))
                    {
                        var status = AzureAppServiceDeploymentStatus.ReadFrom(statusFile);
                        if (status.LastSuccessEndTime.HasValue)
                        {
                            var local = status.LastSuccessEndTime.Value.ToLocalTime().DateTime;
                            ld = local.ToShortDateString() + " " + local.ToLongTimeString();
                            dictionary.Add("DeploymentStatus", $"{status.Status}");
                            //dictionary.Add("DeploymentUpTime", status.Age.ToString());
                        }
                    }
                }

                dictionary.Add("LastDeployment", ld);

                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(HealthStatus.Healthy, description: "Azure Kudu settings", data: roDict);
                return r;
            });

            healthChecksBuilder.AddCheck("Features", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();
                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                        { $"{nameof(settings.Features.Stories)}", settings.Features.Stories },
                        { $"{nameof(settings.Features.UserProfile)}", settings.Features.UserProfile },
                        { $"{nameof(settings.Features.Events)}", settings.Features.Events },
                        { $"{nameof(settings.Features.AdminTrailUpload)}", settings.Features.AdminTrailUpload },
                        { $"{nameof(settings.Features.UserTrailUpload)}", settings.Features.UserTrailUpload },
                        { $"{nameof(settings.Features.Login)}", settings.Features.Login },
                        { $"{nameof(settings.Features.TrailAnalyzer)}", settings.Features.TrailAnalyzer },
                        { $"{nameof(settings.Features.ShareOnFacebook)}", settings.Features.ShareOnFacebook },
                        { $"{nameof(settings.Features.Trails)}", settings.Features.Trails }
                };

                ReadOnlyDictionary<string, object> roDict = new ReadOnlyDictionary<string, object>(dictionary);
                HealthCheckResult r = new HealthCheckResult(HealthStatus.Healthy, description: "feature toggles", data: roDict);
                return r;
            });


            return services;
        }

        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, AppSettings appSettings)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));
            if (!appSettings.TrailContextDisabled)
            {
                services.AddDbContext<TrailContext>(options =>
                {
                    var isp = services.BuildServiceProvider();
                    var settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                    string connectionStringResolved = settings.ConnectionStrings.GetResolvedTrailDBConnectionString();
                    if (string.IsNullOrEmpty(connectionStringResolved))
                    {
                        throw new InvalidOperationException($"ConnectionString not set for {nameof(settings.ConnectionStrings.TrailDB)}");
                    }
                    options.UseSqlite(connectionStringResolved);
                });
            }

            if (!appSettings.IdentityContextDisabled)
            {
                services.AddDbContext<IdentityContext>(options =>
                {
                    var isp = services.BuildServiceProvider();
                    var settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                    string connectionStringResolved = settings.ConnectionStrings.GetResolvedIdentityDBConnectionString();
                    if (string.IsNullOrEmpty(connectionStringResolved))
                    {
                        throw new InvalidOperationException($"ConnectionString not set for {nameof(settings.ConnectionStrings.IdentityDB)}");
                    }
                    options.UseSqlite(connectionStringResolved);
                });
            }
            if (!appSettings.TaskContextDisabled)
            {
                services.AddDbContext<TaskContext>(options =>
                {
                    var isp = services.BuildServiceProvider();
                    var settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                    string connectionStringResolved = settings.ConnectionStrings.GetResolvedTaskDBConnectionString();
                    if (string.IsNullOrEmpty(connectionStringResolved))
                    {
                        throw new InvalidOperationException($"ConnectionString not set for {nameof(settings.ConnectionStrings.TaskDB)}");
                    }
                    options.UseSqlite(connectionStringResolved);
                });
            }

            return services;
        }

        public static Task EnsureUserAsync(OAuthCreatingTicketContext oauthContext, IServiceProvider serviceProvider)
        {
            if (oauthContext == null)
            {
                throw new ArgumentNullException(nameof(oauthContext));
            }
            return EnsureUserAsync(oauthContext.Principal, serviceProvider, false, null);
        }

        public static Task<Guid> EnsureUserAsync(ClaimsPrincipal principal, IServiceProvider serviceProvider, bool createRolesFromPrincipal, Guid? proposeduserID)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var logger = serviceProvider.GetRequiredService<ILogger<ClaimsTransformer>>(); //Missbrauch des Claimstransformers hier, da es die erste Hälfte der selben Operation ist!

            var context = serviceProvider.GetRequiredService<IdentityContext>();

            if (context.TryGetFederatedIdentity(principal, out var identity) == false)
            {
                string email = principal.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    Exception ex = new InvalidOperationException("ClaimsPrincipal has no Claimtype==EMail"); //sollte NIE auftreten aber wenn doch schwere Exception
                    logger.LogError(ex, "EnsureUser-Issue"); //sende explizit ans Logging System. Diese Exception passiert tief in der Middleware, sodass ich mir (noch) nicht sicher bin, dass sie von der AI-Middleware geloggt wird.
                    throw ex;
                }

                var created = context.CreateIdentity(principal, createRolesFromPrincipal, proposeduserID);
                context.SaveChanges();
                return Task.FromResult(created.UserID);
            }
            Guard.AssertNotNull(identity);
            return Task.FromResult(identity.UserID); //user existiert, alles OK, Aufgabe erledigt!
        }

        private static async Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body>");
            await context.Response.WriteAsync("A remote failure has occurred: <br>" +
                context.Failure.Message.Split(Environment.NewLine).Select(s => HtmlEncoder.Default.Encode(s) + "<br>").Aggregate((s1, s2) => s1 + s2));

            if (context.Properties != null)
            {
                await context.Response.WriteAsync("Properties:<br>");
                foreach (var pair in context.Properties.Items)
                {
                    await context.Response.WriteAsync($"-{ HtmlEncoder.Default.Encode(pair.Key)}={ HtmlEncoder.Default.Encode(pair.Value)}<br>");
                }
            }

            await context.Response.WriteAsync("<a href=\"/\">Home</a>");
            await context.Response.WriteAsync("</body></html>");

            // context.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(context.Failure.Message));

            context.HandleResponse();
        }

        public static void ConfigureAuthentication(this IServiceCollection services, AppSettings settings)
        {
            //WM 21.12.2019: https://github.com/aspnet/AspNetCore/blob/master/src/Security/Authentication/samples/SocialSample/Startup.cs

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (!settings.StaticUserSettingsEnabled)
            {
                //ClaimsTransformation add Role Information the the Context.CurrentUser
                services.AddTransient<IClaimsTransformation>((serviceProvider) => new ClaimsTransformer(serviceProvider.GetRequiredService<IdentityContext>(), serviceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>(), serviceProvider.GetRequiredService<ILogger<ClaimsTransformer>>()));
            }

            var authBuilder = services.AddAuthentication(generalOptions =>
            {
                //https://stackoverflow.com/questions/52492666/what-is-the-point-of-configuring-defaultscheme-and-defaultchallegescheme-on-asp
                generalOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                generalOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                generalOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(cookieOptions =>
            {
                //https://dotnetcoretutorials.com/2017/05/13/cookie-authentication-asp-net-core/
                //cookie auth is the "persistence-feature for the other auth methods (Google, Facbook, Custom)
                cookieOptions.LoginPath = "/auth/signin";
                cookieOptions.LogoutPath = "/auth/signout";
            });

            if (settings.GoogleAuthentication)
            {
                if (settings.GoogleSettings == null)
                {
                    throw new InvalidOperationException("Google authentication is enabled via AppSettings but settings are missing");
                }

                authBuilder.AddGoogle(googleOptions =>
                  {
                      googleOptions.CallbackPath = "/signin-google";
                      googleOptions.ClientId = settings.GoogleSettings.ClientId;
                      googleOptions.ClientSecret = settings.GoogleSettings.ClientSecret;
                      //googleOptions.SignInScheme defaults to generalOptions.DefaultSignInScheme => CookieAuthenticationDefaults.AuthenticationScheme
                      googleOptions.Events = new OAuthEvents()
                      {
                          OnCreatingTicket = (cntx => EnsureUserAsync(cntx, services.BuildServiceProvider())),
                          OnRemoteFailure = HandleOnRemoteFailure
                      };

                      //WM 21.12.2019 new options found on the official Asp.Net Core samples
                      googleOptions.AuthorizationEndpoint += "?prompt=consent"; // Hack so we always get a refresh token, it only comes on the first authorization response
                      googleOptions.AccessType = "offline";
                      googleOptions.SaveTokens = true;
                  });
            }

            if (settings.GitHubAuthentication)
            {
                if (settings.GitHubSettings == null)
                {
                    throw new InvalidOperationException("GitHub authentication is enabled via AppSettings but settings are missing");
                }

                // You must first create an app with GitHub and add its ID and Secret to your user-secrets.
                // https://github.com/settings/applications/
                // https://developer.github.com/apps/building-oauth-apps/authorizing-oauth-apps/
                authBuilder.AddOAuth("GitHub", "Github", o =>
                 {
                     o.ClientId = settings.GitHubSettings.ClientId;
                     o.ClientSecret = settings.GitHubSettings.ClientSecret;
                     o.CallbackPath = "/signin-github";
                     o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                     o.TokenEndpoint = "https://github.com/login/oauth/access_token";
                     o.UserInformationEndpoint = "https://api.github.com/user";
                     o.ClaimsIssuer = "OAuth2-Github";
                     o.SaveTokens = true;
                     //// Retrieving user information is unique to each provider.
                     o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                     o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                     o.ClaimActions.MapJsonKey("urn:github:name", "name");
                     o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
                     //o.ClaimActions.MapJsonKey("urn:github:url", "url");
                     o.Events = new OAuthEvents
                     {
                         OnRemoteFailure = HandleOnRemoteFailure,
                         //OnCreatingTicket = (cntx => EnsureUser(cntx, services.BuildServiceProvider())),

                         OnCreatingTicket = async context =>
                         {
                             // Get the GitHub user
                             var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                             request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                             var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                             response.EnsureSuccessStatusCode();

                             using (var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
                             {
                                 context.RunClaimActions(user.RootElement);
                             }
                             await EnsureUserAsync(context, services.BuildServiceProvider());
                         }
                     };
                 });
            }

            if (settings.FacebookAuthentication)
            {
                if (settings.FacebookSettings == null)
                {
                    throw new InvalidOperationException("Facebook authentication is enabled via AppSettings but settings are missing");
                }
                authBuilder.AddFacebook(facebookOptions =>
                  {
                      facebookOptions.CallbackPath = "/signin-facebook";
                      facebookOptions.ClientId = settings.FacebookSettings.AppId;
                      facebookOptions.ClientSecret = settings.FacebookSettings.AppSecret;

                      //facebookOptions.SignInScheme defaults to generalOptions.DefaultSignInScheme => CookieAuthenticationDefaults.AuthenticationScheme
                      facebookOptions.Events = new OAuthEvents()
                      {
                          OnCreatingTicket = (cntx => EnsureUserAsync(cntx, services.BuildServiceProvider())),
                          OnRemoteFailure = HandleOnRemoteFailure
                      };
                  });
            }
        }
    }
}
