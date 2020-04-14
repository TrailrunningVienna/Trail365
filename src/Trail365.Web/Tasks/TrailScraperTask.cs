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
    public class TrailScraperTask : BackgroundTask
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
            var scopedTrail = await scopedDB.Trails.Include(t => t.AnalyzerBlob).Where(t => t.ID == this.Trail.ID).SingleOrDefaultAsync();
            await this.CalculateTrailPreview(scopedTrail, scopedDB, scopedScrapingService, this.Context.Url, this.Context.DefaultLogger);
            var dbchanges = await scopedDB.SaveChangesAsync();
            this.Context.DefaultLogger.LogTrace($"DBContext.SaveChanges={dbchanges} ({this.Trail.Name})");
        }

        private Task CalculateTrailPreview(Trail trail, TrailContext _context, ScrapingService _scrapingService, IUrlHelper url, ILogger logger)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (trail == null) throw new ArgumentNullException(nameof(trail));

            Guard.AssertNotNull(trail.AnalyzerBlob);
            Guard.AssertNotNull(logger, nameof(logger));
            logger.LogTrace($"{nameof(this.CalculateTrailPreview)}.Start {trail.Name}");
            _scrapingService.ScrapeTrail(_context, trail, url);
            logger.LogTrace($"{nameof(this.CalculateTrailPreview)}.End {trail.Name}");
            return Task.CompletedTask;
        }

    }

}
