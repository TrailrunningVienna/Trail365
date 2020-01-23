using System;
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
            return base.Ok(new { Status = "Ok", Comment = "Task Scheduled" });
        }

        [Route("fbsync")]
        public IActionResult StartFacebookSync()
        {
            BackgroundTaskFactory.CreateTask<FacebookSyncTask>(this._serviceScopeFactory, this.Url, this._logger).Queue(this._queue);
            return base.Ok(new { Status = "Ok",Comment = "Task scheduled" });
        }

        [Route("seedstories")]
        public IActionResult SeedStories([FromServices] TrailContext context, [FromServices]BlobService blobService)
        {
            context.SeedStories(StoryDtoProvider.UniqueStories(), blobService, this.Url, StoryStatus.Default);
            return base.Ok(new { Status = "Ok", Comment ="Completed" });
        }

        [Route("seedplaces")]
        public IActionResult SeedPlaces([FromServices] TrailContext context, [FromServices]BlobService blobService)
        {
            var dtoProvider = PlaceDtoProvider.CreateInstance();
            context.SeedPlaces(dtoProvider, this.Url);
            return base.Ok(new { Status = "Ok", Comment = "Completed" });
        }


        [Route("seedevents")]
        public IActionResult SeedEvents([FromServices] TrailContext context, [FromServices]BlobService blobService)
        {
            var dtoProvider = EventDtoProvider.CreateFromEventDtos(EventDtoProvider.VipavaValley(), EventDtoProvider.IATF2020());
            context.SeedEvents(dtoProvider, blobService, this.Url);
            return base.Ok(new { Status = "Ok", Comment = "Completed" });
        }



    }
}