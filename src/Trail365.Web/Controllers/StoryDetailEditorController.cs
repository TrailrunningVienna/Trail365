using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    [Authorize(Roles = "Admin,User,Member,Moderator")]
    public class StoryDetailEditorController : Controller
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger _logger;

        public StoryViewModel InitStoryViewModel(Guid storyID, bool includeImages)
        {
            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            StoryQueryFilter sqf = StoryQueryFilter.GetByID(storyID, true, login.GetListAccessPermissionsForCurrentLogin());
            var story = _context.GetStoriesByFilter(sqf).SingleOrDefault();

            if (story == null)
            {
                throw new InvalidOperationException($"Story with ID '{storyID}' does not exist.");
            }
            return story.ToStoryViewModel(login);
        }

        public StoryDetailEditorController(TrailContext context, BlobService blobService, ILogger<StoryDetailsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Edit single storyBlock
        /// </summary>
        /// <param name="id">StoryDetailID! (=StoryBlock)</param>
        /// <returns></returns>
        public IActionResult Edit(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.NotFound();
            }
            StoryBlock item;
            using (var t = _context.DependencyTracker("GetStoryBlock"))
            {
                item = _context.StoryBlocks.Where(storyBlock => storyBlock.ID == id).Include(sb => sb.Image).SingleOrDefault();
            }

            if (item == null)
            {
                return this.NotFound();
            }

            var model = item.ToStoryBlockViewModel();

            this.ViewData.CreateBlockTypeSelectList("BlockType", model.BlockType);
            return this.View(model);
        }

        public IActionResult Insert(string relative, Guid relativeid)
        {
            var relativePosition = _context.StoryBlocks.Find(relativeid);

            if (relativePosition == null)
            {
                return this.NotFound();
            }

            var insertedBlock = _context.InsertStoryBlock(InsertMode.After, relativePosition.StoryID, relativePosition.ID);
            _context.SaveChanges();

            var model = insertedBlock.ToStoryBlockViewModel();

            this.ViewData.CreateBlockTypeSelectList("BlockType", model.BlockType);

            return this.View("Edit", model);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind()] StoryBlockViewModel viewModel)
        {
            if (id != viewModel.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.StoryBlocks.Where(e => e.ID == id).SingleOrDefault();

                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    viewModel.ApplyChangesTo(loaded);
                    loaded.ModifiedUtc = DateTime.UtcNow;
                    loaded.ModifiedByUser = System.Threading.Thread.CurrentPrincipal?.Identity.Name;
                    _context.Update(loaded);
                    _context.SaveChanges();
                    viewModel = loaded.ToStoryBlockViewModel();

                    this.ViewData.CreateBlockTypeSelectList("BlockType", viewModel.BlockType);
                    return this.View(viewModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.BlockExists(viewModel.ID))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            this.ViewBag.Warning = "Invalid Model State";
            return this.View(viewModel);
        }

        private bool BlockExists(Guid id)
        {
            return _context.StoryBlocks.Any(e => e.ID == id);
        }

        public IActionResult Delete(Guid? id)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.NotFound();
            }
            var item = _context.StoryBlocks.SingleOrDefault(t => t.ID == id);

            if (item == null)
            {
                return this.NotFound();
            }

            var model = item.ToStoryBlockViewModel();

            return this.View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var item = _context.StoryBlocks.FirstOrDefault(t => t.ID == id);
            if (item == null)
            {
                return base.NotFound();
            }
            _context.DeleteStoryBlock(item, _blobService);
            _context.SaveChanges();
            this.TempData["Info"] = "Block deleted!";
            return this.RedirectToAction("Index", "StoryDetails", new { id = item.StoryID });
        }
    }
}
