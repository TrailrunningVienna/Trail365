using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Services;
using Trail365.Tasks;
using Trail365.Web.Tasks;

namespace Trail365.Web.Api.Controllers
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class TrailsController : ControllerBase
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger _logger;
        private readonly TrailImporterService _importService;
        private readonly IBackgroundTaskQueue _queue;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TrailsController(TrailContext context, BlobService blobService, ILogger<TrailsController> logger, TrailImporterService importService, IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService;
            //_scrapingService = scrapingWorker;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        //api/trail(s)/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateTracksPost([FromForm] IFormFile file, [FromQuery(Name = "source")]string source)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentNullException(nameof(source));
            if (file.Length < 1) return this.BadRequest("File length == 0");
            //bool scraping = false;
            using (var stream = file.OpenReadStream())
            {
                using (var reader = new StreamReader(stream, true))
                {
                    string xml = await reader.ReadToEndAsync();
                    var result = _importService.Execute(_context, xml, source, System.DateTime.UtcNow, this.Url);
                }
            }
            return base.CreatedAtAction(nameof(CreateTracksPost), new { id = "id1" });
        }

        /// <summary>
        /// Get: api/preview
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("preview/{id}")]
        public async Task<ActionResult<TrailDto>> Preview(Guid id)
        {
            var trail = await _context.Trails.FindAsync(id);
            if (trail == null)
            {
                return this.NotFound();
            }

            var task = BackgroundTaskFactory.CreateTask<TrailPreviewTask>(this._serviceScopeFactory, this.Url);
            task.Trail = trail;
            task.Queue(this._queue);
            return trail.ToTrailDto();
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<TrailDto>>> GetTrails()
        {
            return await _context.Trails.Select(t => t.ToTrailDto()).ToListAsync();
        }

        // GET: api/Trail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Trail>> GetTrail(Guid id)
        {
            var trail = await _context.Trails.FindAsync(id);

            if (trail == null)
            {
                return this.NotFound();
            }

            return trail;
        }

        // PUT: api/trails/5
        [HttpPut("{id}")]
        public IActionResult PutTrail(Guid id, Trail trail)
        {
            if (id != trail.ID)
            {
                return this.BadRequest();
            }

            _context.Entry(trail).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TrailExists(id))
                {
                    return this.NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.NoContent();
        }

        /// <summary>
        /// POST: api/Trail/raw
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        [HttpPost("raw")]
        public async Task<ActionResult<Trail>> PostRawTrail([FromBody]Trail trail)
        {
            if (!this.ModelState.IsValid) return this.BadRequest();
            _context.Trails.Add(trail);
            if (trail.GpxBlob != null)
            {
                _context.Blobs.Add(trail.GpxBlob);
            }
            await _context.SaveChangesAsync();
            return this.CreatedAtAction("GetTrail", new { id = trail.ID }, trail);
        }

        private void AssignGpxData(Trail trailEntity, string gpxData)
        {
            trailEntity.GpxBlob = new Blob();
            trailEntity.GpxBlobID = trailEntity.GpxBlob.ID;
            BlobMapping mapping = _blobService.UploadXml(gpxData, trailEntity.GpxBlob.ID, this.Url); //gpx stored inside blobStorage => required for scraping!
            mapping.ApplyToBlob(trailEntity.GpxBlob);
            TrailExtender.ReadGpxFileInfo(gpxData, trailEntity); //overwrites "Name" and "InternalDescription"
            _context.Trails.Add(trailEntity);
            _context.Blobs.Add(trailEntity.GpxBlob);
        }

        /// <summary>
        /// ~/api/trail
        /// expects a Dto including gpx-file content as byte[]
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        [HttpPost("")]
        public ActionResult<TrailDto> PostTrail([FromBody]TrailDto trail)
        {
            _logger.LogTrace("PostTrail.Start");

            Trail trailEntity = new Trail
            {
                ID = trail.ID,
                Description = trail.Description,
                InternalDescription = trail.InternalDescription,
                GpxDownloadAccess = trail.GpxDownloadAccess,
                ListAccess = trail.ListAccess,
                Excerpt = trail.Excerpt,
            };

            string xmlGpx = System.Text.Encoding.UTF8.GetString(trail.Gpx);

            this.AssignGpxData(trailEntity, xmlGpx);

            if (string.IsNullOrEmpty(trail.Name) == false)
            {
                trailEntity.Name = trail.Name; // if there is a name on the input then it wins!
            }

            _context.SaveChanges();

            var task = BackgroundTaskFactory.CreateTask<TrailPreviewTask>(this._serviceScopeFactory, this.Url);
            task.Trail = trailEntity;
            task.Queue(this._queue);

            _logger.LogTrace("PostTrail.Return");
            return trailEntity.ToTrailDto();
        }

        // DELETE: api/Trail/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TrailDto>> DeleteTrail(Guid id)
        {
            var trail = await _context.Trails.FindAsync(id);
            if (trail == null)
            {
                return this.NotFound();
            }
            _context.DeleteTrail(trail, _blobService);
            await _context.SaveChangesAsync();
            return trail.ToTrailDto();
        }
    }
}
