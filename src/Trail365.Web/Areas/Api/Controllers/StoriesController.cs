using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Services;

namespace Trail365.Web.Api.Controllers
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger _logger;

        public StoriesController(TrailContext context, BlobService blobService, ILogger<StoriesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/stories
        [HttpGet("")]
        public ActionResult<IEnumerable<StoryDto>> GetStories()
        {
            var filter = new StoryQueryFilter();
            return _context.GetStoriesByFilter(filter).Select(s => s.ToStoryDto()).ToList();
        }

        // GET: api/Trail/5
        [HttpGet("{id}")]
        public ActionResult<StoryDto> GetStory(Guid id)
        {
            var story = _context.GetStoriesByFilter(StoryQueryFilter.GetByID(id, false)).SingleOrDefault();
            if (story == null)
            {
                return this.NotFound();
            }

            return story.ToStoryDto();
        }

        //// PUT: api/Trail/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTrail(Guid id, Trail trail)
        //{
        //    if (id != trail.ID)
        //    {
        //        return this.BadRequest();
        //    }

        //    _context.Entry(trail).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!this.TrailExists(id))
        //        {
        //            return this.NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return this.NoContent();
        //}

        //// POST: api/Trail/raw
        //[HttpPost("raw")]
        //public async Task<ActionResult<Story>> PostRawTrail([FromBody]Trail trail)
        //{
        //    if (!this.ModelState.IsValid) return this.BadRequest();
        //    _context.Trails.Add(trail);
        //    await _context.SaveChangesAsync();
        //    return this.CreatedAtAction("GetTrail", new { id = trail.ID }, trail);
        //}

        /// <summary>
        /// ~/api/trail
        /// expects a Dto including gpx-file content as byte[]
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        [HttpPost("")]
        public ActionResult<StoryDto> PostStory([FromBody]StoryDto storyDto)
        {
            _logger.LogTrace("PostStory.Start");
            if (storyDto == null) throw new ArgumentNullException(nameof(storyDto));
            var result = _blobService.CreateStoryFromStoryDto(storyDto, this.Url);
            _context.StoryBlocks.AddRange(result.Item1.StoryBlocks);
            _context.Blobs.AddRange(result.Item1.StoryBlocks.Where(bl => bl.Image != null).Select(p => p.Image));
            _context.Stories.Add(result.Item1);
            _context.SaveChanges();
            _logger.LogTrace("PostStory.Return");
            return result.Item2;
        }

        // DELETE: api/story/5
        [HttpDelete("{id}")]
        public ActionResult<StoryDto> DeleteStory(Guid id)
        {
            var story = _context.GetStoriesByFilter(StoryQueryFilter.GetByID(id, false)).SingleOrDefault();

            if (story == null)
            {
                return this.NotFound();
            }

            _context.DeleteStory(story, _blobService);
            _context.SaveChanges();
            return story.ToStoryDto();
        }
    }
}
