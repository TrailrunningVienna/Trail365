using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;
using Trail365.Tasks;

namespace Trail365.Web.Tasks
{
    /// <summary>
    /// Background Task for Scraping AND Elevationprofile calculation
    /// </summary>
    public class TrailPreviewTask : BackgroundTask
    {
        public Trail Trail { get; set; }

        protected override void OnBeforeExecute()
        {
            if (this.Trail != null)
            {
                this.Caption = $"{this.Trail.Name}";
            }
            else
            {
                this.Caption = "<Unknown Trail";
            }
            var scopedScrapingService = this.Context.ServiceProvider.GetRequiredService<ScrapingService>();
            this.Caption += $" ({scopedScrapingService.Scraper.GetType().Name})";
        }

        protected override async Task Execute(CancellationToken cancellationToken)
        {
            var scopedDB = this.Context.ServiceProvider.GetRequiredService<TrailContext>();
            var scopedScrapingService = this.Context.ServiceProvider.GetRequiredService<ScrapingService>();
            var scopedTrail = await scopedDB.Trails.Include(t => t.GpxBlob).Where(t => t.ID == this.Trail.ID).SingleOrDefaultAsync();
            await this.CalculateTrailPreview(scopedTrail, scopedDB, scopedScrapingService, this.Context.Url, this.Context.DefaultLogger);
            var dbchanges = await scopedDB.SaveChangesAsync();
            this.Context.DefaultLogger.LogTrace($"{nameof(TrailPreviewTask)}.DBContext.SaveChanges={dbchanges} ({this.Trail.Name})");
        }

        private async Task CalculateTrailPreview(Trail trail, TrailContext _context, ScrapingService _scrapingService, IUrlHelper url, ILogger logger)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (trail == null) throw new ArgumentNullException(nameof(trail));

            Guard.AssertNotNull(trail.GpxBlob);
            Guard.AssertNotNull(logger, nameof(logger));
            logger.LogTrace($"{nameof(this.CalculateTrailPreview)}.Start {trail.Name}");
            var getGpxXmlTask = ServiceHelper.GetGpxXml(trail.GpxBlob.Url, logger); //start async download before Scraping is started!
            _scrapingService.ScrapeTrail(_context, trail, url);
            string gpxXml = await getGpxXmlTask;
            _scrapingService.BuildElevationProfile(_context, trail, gpxXml, url);
            logger.LogTrace($"{nameof(this.CalculateTrailPreview)}.End {trail.Name}");
        }
    }
}
