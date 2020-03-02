using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSS.GraphQL.Facebook;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Services;
using Trail365.Tasks;

namespace Trail365.Web.Tasks
{
    public class FacebookSyncTask : BackgroundTask
    {
        protected override Task Execute(CancellationToken cancellationToken)
        {
            this.TrailContext = this.Context.ServiceProvider.GetRequiredService<TrailContext>();
            var settings = this.Context.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>().CurrentValue;
            var scopedLogger = this.Context.ServiceProvider.GetRequiredService<ILogger<FacebookSyncTask>>();
            var blobService = this.Context.ServiceProvider.GetRequiredService<BlobService>();
            return this.GetSync(settings, scopedLogger, this.Context.Url, blobService);
        }

        private TrailContext TrailContext { get; set; }

        private async Task GetSync(AppSettings _settings, ILogger logger, IUrlHelper helper, BlobService blobService)
        {
            string fb_ID = _settings.FacebookSettings.ImporterId;
            string fb_token = _settings.FacebookSettings.ImporterAccessToken;
            int days = _settings.FacebookSettings.ImporterDays;
            if (string.IsNullOrEmpty(fb_ID)) throw new InvalidOperationException("FacebookSettings for Importer are missing (ID)");
            if (string.IsNullOrEmpty(fb_token)) throw new InvalidOperationException("FacebookSettings for Importer are missing (Token)");
            DateTimeOffset since = new DateTimeOffset(DateTime.Now.AddDays(-days).Date);

            this.Context.DefaultLogger.LogInformation($"{nameof(FacebookSyncTask)} started (ApiDelay={_settings.FacebookSettings.ApiDelayMilliseconds}ms, since={since.Date.ToLongDateString()})");

            FacebookDataExtractor extractor = new FacebookDataExtractor
            {
                DelayMilliseconds = _settings.FacebookSettings.ApiDelayMilliseconds
            };

            FacebookEventImporter eventImporter;

            List<FacebookEventDescriptor> results = new List<FacebookEventDescriptor>();
            int changes;
            extractor.TracerDelegate = (ll, msg) =>
            {
                this.Context.DefaultLogger.Log(LogLevel.Trace, msg); //TODO translate and filter LogLevel!
            };

            try
            {
                int stepSize = 30;

                DateTimeOffset to = new DateTimeOffset(DateTime.Now.AddDays(days).Date);
                DateTimeOffset current = since;

                while (current < to)
                {
                    DateTimeOffset next = current.AddDays(stepSize);
                    this.Context.DefaultLogger.LogDebug($"Start FB-Read from {current.Date.ToShortDateString()} to {next.Date.ToShortDateString()}{Environment.NewLine}");
                    var range = extractor.GetEventDescriptionsByOwnerId(fb_token, fb_ID,
                        (d) =>
                        {
                            return !results.Select(r => r.Id).Contains(d.Id);
                        }, current, next, CancellationToken.None).ToArray();

                    this.Context.DefaultLogger.LogDebug($"FB-Read completed with {range.Length} findings{Environment.NewLine}");
                    results.AddRange(range);
                    current = next.AddDays(1);
                }

                eventImporter = new FacebookEventImporter(this.TrailContext, helper, HttpClientDownloadService.DefaultInstance, blobService);

                System.Text.StringBuilder importerLog = new System.Text.StringBuilder();

                try
                {
                    await eventImporter.Import(new FacebookEventData
                    {
                        ExternalSource = $"fb-{fb_ID}".ToLowerInvariant(),
                        Events = results.ToArray(),
                    }, this.Context.DefaultLogger);
                }
                finally
                {
                    changes = this.TrailContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
