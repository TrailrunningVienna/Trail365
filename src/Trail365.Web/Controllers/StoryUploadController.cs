using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    public class StoryUploadController : Controller
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;

        /// <summary>
        /// Main entrypoint/view for multi file upload
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult MultiBlockUpload()
        {
            var model = this.InitStory(null, null);
            return this.View("MultiBlockUpload", model);
        }

        [HttpPost]
        public IActionResult MultiBlockUpload(StoryCreationViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);

            var story = _context.GetStoriesByFilter(StoryQueryFilter.GetByID(model.ID, false)).SingleOrDefault();

            if (story == null)
            {
                return this.NotFound();
            }

            story.Status = StoryStatus.Draft;

            Guard.Assert(story.Status != StoryStatus.Upload);

            if (!string.IsNullOrEmpty(model.Name))
            {
                story.Name = model.Name;
            }
            else
            {
                if (string.IsNullOrEmpty(story.Name))
                {
                    story.Name = "My story...";
                }
            }
            _context.SaveChanges();

            return this.Redirect(this.Url.GetStoryUrl(story.ID));
        }

        public StoryUploadController(TrailContext context, BlobService blobService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public IActionResult RemoveBlock(Guid id, Guid storyid)
        {
            var block = _context.StoryBlocks.Include(sb => sb.Image).Where(sb => sb.ID == id).SingleOrDefault();

            if (block == null)
            {
                return this.NotFound();
            }

            if (block.StoryID != storyid)
            {
                throw new InvalidOperationException("Block does not belongs to the requested story");
            }

            _context.DeleteStoryBlock(block, this._blobService);
            _context.SaveChanges();

            var model = this.InitStory(null, storyid);

            var currentBlocks = _context.StoryBlocks.Include(sb => sb.Image).Where(sb => sb.StoryID == storyid).OrderBy(sb => sb.SortOrder).ToList();

            model.FileInfos = currentBlocks.Select(s => new StoryBlockFileViewModel
            {
                ID = s.ID,
                AbsoluteUrl = s.Image.Url,
                FileName = s.Image.OriginalFileName,
                Extension = System.IO.Path.GetExtension(s.Image.OriginalFileName),
            }).ToList();

            return this.PartialView("_MultiBlockUpload", model);
        }

        public StoryCreationViewModel InitStory(StoryCreationViewModel model, Guid? storyID)
        {
            if (model == null)
            {
                model = new StoryCreationViewModel();
            }
            model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            if (storyID.HasValue)
            {
                model.ID = storyID.Value;
            }
            return model;
        }

        public IActionResult UploadBlocks(IFormCollection collection, Guid id)
        {
            var newFiles = collection.Files.ToArray();
            var existingFilesJson = collection["existingFiles"];

            var existingFiles = JsonConvert.DeserializeObject<List<StoryBlockFileViewModel>>(existingFilesJson);

            Story story = _context.GetStoriesByFilter(StoryQueryFilter.GetByID(id, true)).SingleOrDefault();

            if (story == null)
            {
                story = new Story { Name = id.ToString(), ID = id, Status = StoryStatus.Upload };
                _context.Stories.Add(story);
            }
            else
            {
                _context.Stories.Update(story);
            }

            var model = this.InitStory(null, story.ID);

            if (existingFiles != null)
            {
                model.FileInfos.AddRange(existingFiles);
            }

            var results = _blobService.AppendBlocks(_context, story, newFiles, this.Url);
            _context.SaveChanges();
            model.FileInfos.AddRange(results);
            return this.PartialView("_MultiBlockUpload", model);
        }
    }
}
