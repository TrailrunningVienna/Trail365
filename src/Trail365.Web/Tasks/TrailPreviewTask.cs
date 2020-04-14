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
        }

        protected override Task Execute(CancellationToken cancellationToken)
        {
            this
                .ContinueWith<TrailAnalyzerTask>(ta => ta.Trail = this.Trail)
                .ContinueWith<TrailElevationProfilerTask>(ta => ta.Trail = this.Trail)
                .ContinueWith<TrailScraperTask>(ta => ta.Trail = this.Trail);
            return Task.CompletedTask;
        }

    }
}
