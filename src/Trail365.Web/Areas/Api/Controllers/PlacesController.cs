using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.DTOs;

namespace Trail365.Web.Api.Controllers
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly TrailContext _context;
        private readonly ILogger _logger;

        public PlacesController(TrailContext context, ILogger<PlacesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/place
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<PlaceDto>>> GetPlaces()
        {
            return await _context.Places.Select(p => p.ToPlaceDto()).ToListAsync();
        }

        // GET: api/event/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlaceDto>> GetPlace(Guid id)
        {
            var entity = await _context.Places.FindAsync(id);

            if (entity == null)
            {
                return this.NotFound();
            }
            return entity.ToPlaceDto();
        }

        /// <summary>
        /// ~/api/place
        /// expects a Dto including gpx-file content as byte[]
        /// </summary>
        /// <param name="eventDto"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<ActionResult<PlaceDto>> PostPlace([FromBody]PlaceDto placeDto)
        {
            _logger.LogTrace("PostPlace.Start");
            _context.AddPlace(placeDto);
            await _context.SaveChangesAsync();
            _logger.LogTrace("PostPlace.Return");
            var model = _context.Places.Single(e => e.ID == placeDto.ID);
            return model.ToPlaceDto();
        }

        // DELETE: api/event/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<PlaceDto>> DeletePlace(Guid id)
        {
            var model = await _context.Places.FindAsync(id);

            if (model == null)
            {
                return this.NotFound();
            }
            _context.DeletePlace(model);
            await _context.SaveChangesAsync();
            return model.ToPlaceDto();
        }
    }
}
