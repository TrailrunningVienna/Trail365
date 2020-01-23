using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Internal;
using Trail365.Services;

namespace Trail365.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger _logger;

        public ImageController(TrailContext context, BlobService blobService, ILogger<ImageController> logger)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<BlobDto> PostImage([FromBody]BlobDto image)
        {
            _logger.LogTrace("PostImage.Start");
            try
            {
                if (image == null) return this.BadRequest();
                if ((image.Data == null) || (image.Data.Length == 0)) return this.BadRequest();
                var result = _blobService.CreateBlobFromBlobDto(image, this.Url);
                Guard.Assert(result.Item1.ID == result.Item2.ID);
                Guard.Assert(result.Item1.FolderName == result.Item2.SubFolder);
                _context.Blobs.Add(result.Item1);
                _context.SaveChanges();
                _logger.LogTrace("PostImage.Return");
                return this.CreatedAtAction("PostImage", new { id = result.Item2.ID }, result.Item2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostImage.Exception");
                throw;
            }
        }
    }
}
