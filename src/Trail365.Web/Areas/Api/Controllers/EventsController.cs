using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Services;

namespace Trail365.Web.Api.Controllers
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger _logger;
        private readonly IBackgroundTaskQueue _queue;

        public EventsController(TrailContext context, BlobService blobService, IBackgroundTaskQueue queue, ILogger<EventsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        [Route("")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            return await _context.Events.Select(e => e.ToEventDto()).ToListAsync();
        }

        // GET: api/event/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(Guid id)
        {
            var resultEvent = await _context.Events.FindAsync(id);

            if (resultEvent == null)
            {
                return this.NotFound();
            }

            return resultEvent.ToEventDto();
        }

        // POST: api/event/raw
        //[Route("events")]
        [HttpPost("raw")]
        public async Task<ActionResult<Event>> PostRawEvent([FromBody]Event model)
        {
            if (!this.ModelState.IsValid) return this.BadRequest();
            _context.Events.Add(model);
            await _context.SaveChangesAsync();
            return this.CreatedAtAction("GetEvent", new { id = model.ID }, model);
        }

        /// <summary>
        /// ~/api/trail
        /// expects a Dto including gpx-file content as byte[]
        /// </summary>
        /// <param name="eventDto"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<ActionResult<EventDto>> PostEvent([FromBody]EventDto eventDto)
        {
            _logger.LogTrace("PostEvent.Start");
            _context.AddEvent(this._blobService, this.Url, eventDto);
            await _context.SaveChangesAsync();
            _logger.LogTrace("PostEvent.Return");
            var model = _context.Events.Single(e => e.ID == eventDto.ID);
            return model.ToEventDto();
        }

        // DELETE: api/event/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<EventDto>> DeleteEvent(Guid id)
        {
            var model = await _context.Events.FindAsync(id);

            if (model == null)
            {
                return this.NotFound();
            }
            //TODO Test/Ensure that the place is NOT deleted!
            _context.DeleteEvent(model, _blobService);
            await _context.SaveChangesAsync();
            return model.ToEventDto();
        }
    }
}
