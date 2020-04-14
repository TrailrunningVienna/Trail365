using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.Services;
using Trail365.Tasks;
using Trail365.Web.Tasks;

namespace Trail365.Web.Api.Controllers
{
    [Area("Api")]
    [Route("[area]/[controller]")]

    [ApiController]
    public class ManagementController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ManagementController(IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory, ILogger<ManagementController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        /// <summary>
        /// start/trigger for a FB-Sync. Inventedt to call from Azure Logic App!
        /// </summary>
        /// <returns></returns>
        [Route("Backup")]
        public ActionResult Backup(bool? enforceLogging)
        {
            bool NoLogging = true;

            if (enforceLogging.HasValue)
            {
                NoLogging = !enforceLogging.Value;
            }

            var task = BackgroundTaskFactory.CreateTask<BackupTask>(this._serviceScopeFactory, this.Url, _logger); //we disable TaskLoggingSystem for this, so we should inject ILogger/ApplicationInsight at leat for the Infrastructure!
            task.Queue(this._queue, NoLogging); //logging to TaskLog disabled to prevent endless changes by the sync system:
            return base.Ok(new { Status = "Ok", Comment = "Backup scheduled" });
        }

        [Route("fbsync")]
        public IActionResult StartFacebookSync()
        {
            BackgroundTaskFactory.CreateTask<FacebookSyncTask>(this._serviceScopeFactory, this.Url, this._logger).Queue(this._queue);
            return base.Ok(new { Status = "Ok", Comment = "Facebbook sync scheduled" });
        }

        [Route("seedstories")]
        public IActionResult SeedStories([FromServices] TrailContext context, [FromServices]BlobService blobService)
        {
            var dtoProvider = StoryDtoProvider.UniqueStories();
            context.SeedStories(dtoProvider, blobService, this.Url, StoryStatus.Default);
            return base.Ok(new { Status = "Ok", Comment = $"Story seeding completed ({dtoProvider.All.Length})" });
        }

        [Route("seedplaces")]
        public IActionResult SeedPlaces([FromServices] TrailContext context)
        {
            var dtoProvider = PlaceDtoProvider.CreateInstance();
            context.SeedPlaces(dtoProvider, this.Url);
            return base.Ok(new { Status = "Ok", Comment = $"Place seeding completed ({dtoProvider.All.Length})" });
        }


        [Route("logcleanup")]
        public IActionResult LogCleanup()
        {
            BackgroundTaskFactory.CreateTask<TaskLogCleanupTask>(this._serviceScopeFactory, this.Url, this._logger).Queue(this._queue, disabledLogging: true);
            return base.Ok(new { Status = "Ok", Comment = " LogCleanup scheduled" });
        }



        [Route("seedevents")]
        public IActionResult SeedEvents([FromServices] TrailContext context, [FromServices]BlobService blobService)
        {
            var dtoProvider = EventDtoProvider.CreateDummyForPublicSeeds(250);
            context.SeedEvents(dtoProvider, blobService, this.Url);


            var allTrails = context.Trails.ToArray();
            if (allTrails.Length > 0)
            {
                Random r = new Random();
                dtoProvider.All.ToList().ForEach(s =>
                {
                    var ev = context.Events.Single(e => e.ID == s.ID);
                    int nextTrail = r.Next(-allTrails.Length, allTrails.Length - 1);

                    if (nextTrail > -1)
                    {
                        ev.TrailID = allTrails[nextTrail].ID;
                        context.Events.Update(ev);
                    }
                });
            }
            context.SaveChanges();
            return base.Ok(new { Status = "Ok", Comment = $"Event seeding completed ({dtoProvider.All.Length})" });
        }


        [Route("seedtrails")]
        public IActionResult SeedTrails([FromServices] TrailContext context, [FromServices]BlobService blobService)
        {
            var dtoProvider = TrailDtoProvider.CreateDummyForPublicSeeds(75);
            context.SeedTrails(dtoProvider, blobService, this.Url);
            return base.Ok(new { Status = "Ok", Comment = $"Trail seeding completed ({dtoProvider.All.Length})" });
        }

    }
}
