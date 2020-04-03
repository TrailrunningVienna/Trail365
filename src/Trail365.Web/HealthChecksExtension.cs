using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trail365.Configuration;

namespace Trail365.Web
{
    public static class HealthChecksExtension
    {
        private static JObject EntryToJson(HealthReportEntry report)
        {
            List<JProperty> properties = new List<JProperty>()
            {
               new JProperty("status", report.Status.ToString()),
            };
            if (!string.IsNullOrEmpty(report.Description))
            {
                properties.Add(new JProperty("description", report.Description));
            }
            if (report.Data.Count > 0)
            {
                properties.Add(new JProperty("settings", new JObject(report.Data.Select(p => new JProperty(p.Key, p.Value)))));
            }
            return new JObject(properties.ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="result"></param>
        /// <param name="settings">in case of Environment==Development we print out some settings</param>
        /// <returns></returns>
        public static Task DefaultApiInfoHealthResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var items = result.Entries.Select(re => new Tuple<string, JObject>(re.Key, EntryToJson(re.Value)));
            var jItems = items.Select(i => new JProperty(i.Item1, i.Item2));

            List<object> rootItems = new List<object>()
            {
                new JProperty("Status", result.Status.ToString()),
                new JProperty("Version", Helper.GetProductVersionFromEntryAssembly()),
                new JProperty("ProcessUpTime", Helper.GetUptime()),
                new JProperty("ProcessStartTime", Helper.GetStartTime()),
            };

            rootItems.Add(new JProperty("Components", new JObject(jItems)));

            var json = new JObject(rootItems.ToArray());

            return httpContext.Response.WriteAsync(json.ToString(Formatting.Indented));
        }


        public static void AddChatFeatureStatus(this IHealthChecksBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddCheck("Chat", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                     { nameof(settings.Features.Chat), settings.Features.Chat.ToString() }
                     //{ nameof(settings.TrailExplorerBaseUrl), $"{settings.TrailExplorerBaseUrl}"}
                };

                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;

                ReadOnlyDictionary<string, object> roDict;

                if (env.IsDevelopment())
                {
                    roDict = new ReadOnlyDictionary<string, object>(dictionary);
                }
                else
                {
                    roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                }

                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });
        }


        public static void AddScrapingServiceStatus(this IHealthChecksBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddCheck("ScrapingService", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                     { nameof(settings.PuppeteerEnabled), settings.PuppeteerEnabled.ToString() },
                     { nameof(settings.TrailExplorerBaseUrl), $"{settings.TrailExplorerBaseUrl}"}
                };

                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;

                ReadOnlyDictionary<string, object> roDict;

                if (env.IsDevelopment())
                {
                    roDict = new ReadOnlyDictionary<string, object>(dictionary);
                }
                else
                {
                    roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                }

                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });
        }


        public static void AddBackgroundServiceStatus(this IHealthChecksBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddCheck("BackgroundWorker", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                     { nameof(settings.BackgroundServiceDisabled), settings.BackgroundServiceDisabled.ToString() }
                };

                //TODO ensure that connectionstring for TaskSystem is valid!

                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;


                ReadOnlyDictionary<string, object> roDict;

                if (env.IsDevelopment())
                {
                    roDict = new ReadOnlyDictionary<string, object>(dictionary);
                }
                else
                {
                    roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                }

                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });
        }
        public static void AddStorageFeatureStatus(this IHealthChecksBuilder healthChecksBuilder)
        {
            var serviceProvider = healthChecksBuilder.Services.BuildServiceProvider();
            AppSettings here = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;

            healthChecksBuilder.AddCheck("Storage", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(settings.CloudStorageEnabled), settings.CloudStorageEnabled }
                };

                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;

                ReadOnlyDictionary<string, object> roDict;

                if (env.IsDevelopment())
                {
                    roDict = new ReadOnlyDictionary<string, object>(dictionary);
                }
                else
                {
                    roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                }

                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });

            if (here.CloudStorageEnabled)
            {

                healthChecksBuilder.AddCheck("AzureBlob", () =>
                {
                    var isp = healthChecksBuilder.Services.BuildServiceProvider();
                    AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                    IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                    Dictionary<string, object> dictionary = new Dictionary<string, object>
                    {
                    { nameof(settings.CloudStorageEnabled), settings.CloudStorageEnabled }
                    };

                    var proposedHealthStatatus = HealthStatus.Healthy;
                    string proposedDescription = null;

                    string accountName = "N/A";

                    if (settings.CloudStorageEnabled)
                    {
                        if (CloudStorageAccount.TryParse(settings.ConnectionStrings.GetResolvedCloudStorageConnectionString(), out var account))
                        {
                            accountName = account.Credentials.AccountName;
                        }
                    }
                    //TODO check consistensy for AzureBlob vs. local Blob!
                    dictionary.Add(nameof(settings.CloudStorageContainerName), settings.CloudStorageContainerName);
                    dictionary.Add(nameof(settings.CloudStorageMaxAgeSeconds), settings.CloudStorageMaxAgeSeconds);
                    dictionary.Add("CloudStorageAccount", accountName);

                    ReadOnlyDictionary<string, object> roDict;

                    if (env.IsDevelopment())
                    {
                        roDict = new ReadOnlyDictionary<string, object>(dictionary);
                    }
                    else
                    {
                        roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                    }

                    HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                    return r;
                });
            }
            else
            {
                healthChecksBuilder.AddCheck("FileSystemBlobBlob", () =>
                {
                    var isp = healthChecksBuilder.Services.BuildServiceProvider();
                    AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                    IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                    Dictionary<string, object> dictionary = new Dictionary<string, object>
                    {

                    };

                    var proposedHealthStatatus = HealthStatus.Healthy;
                    string proposedDescription = null;

                    dictionary.Add(nameof(settings.FileSystemBlobServiceRootDirectory), settings.FileSystemBlobServiceRootDirectory);
                    dictionary.Add(nameof(settings.FileSystemBlobServiceBrowserEnabled), settings.FileSystemBlobServiceBrowserEnabled);
                    dictionary.Add(nameof(settings.FileSystemBlobServiceRequestPath), settings.FileSystemBlobServiceRequestPath);

                    ReadOnlyDictionary<string, object> roDict;

                    if (env.IsDevelopment())
                    {
                        roDict = new ReadOnlyDictionary<string, object>(dictionary);
                    }
                    else
                    {
                        roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                    }

                    HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                    return r;
                });
            }
        }

        public static void AddTrailExplorerFeatureStatus(this IHealthChecksBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddCheck("TrailExplorer", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(settings.TrailExplorerBaseUrl), $"{settings.TrailExplorerBaseUrl}" },
                    { $"Features.{nameof(settings.Features.TrailAnalyzer)}", $"{settings.Features.TrailAnalyzer}" }
                };

                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;

                if (settings.Features.TrailAnalyzer)
                {
                    if (string.IsNullOrEmpty(settings.TrailExplorerBaseUrl))
                    {
                        proposedHealthStatatus = HealthStatus.Degraded;
                        proposedDescription = $"Feature '{nameof(settings.Features.TrailAnalyzer)}' is enabled but setting '{nameof(settings.TrailExplorerBaseUrl)}' is empty";
                    }
                }

                ReadOnlyDictionary<string, object> roDict;

                if (env.IsDevelopment())
                {
                    roDict = new ReadOnlyDictionary<string, object>(dictionary);
                }
                else
                {
                    roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                }

                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });
        }


        public static void AddBackupFeatureStatus(this IHealthChecksBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddCheck("Backup", () =>
            {
                var isp = healthChecksBuilder.Services.BuildServiceProvider();
                AppSettings settings = isp.GetRequiredService<IOptions<AppSettings>>().Value;
                IWebHostEnvironment env = isp.GetRequiredService<IWebHostEnvironment>();

                Dictionary<string, object> dictionary = new Dictionary<string, object>
                {
                    { nameof(settings.BackupDirectory), $"{settings.BackupDirectory}" },
                    { nameof(settings.SyncEnabled), $"{settings.SyncEnabled}" }
                };
                dictionary.Add(nameof(settings.SyncOverwriteEnabled), $"{settings.SyncOverwriteEnabled}");
                var proposedHealthStatatus = HealthStatus.Healthy;
                string proposedDescription = null;

                if (settings.SyncEnabled)
                {
                    if (string.IsNullOrEmpty(settings.BackupDirectory))
                    {
                        proposedHealthStatatus = HealthStatus.Degraded;
                        proposedDescription = "BackupDirectory not defined but Sync is enabled!";
                    }
                }

                ReadOnlyDictionary<string, object> roDict;

                if (env.IsDevelopment())
                {
                    roDict = new ReadOnlyDictionary<string, object>(dictionary);
                }
                else
                {
                    roDict = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                }

                HealthCheckResult r = new HealthCheckResult(proposedHealthStatatus, proposedDescription, data: roDict);
                return r;
            });

        }
    }
}
