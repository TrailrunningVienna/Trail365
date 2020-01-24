using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trail365.Data;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    [Authorize(Roles = "Admin,User,Member,Moderator")]
    public class StoryEditorController : Controller
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;

        public IActionResult Delete(Guid? id, string redirectTo)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.NotFound();
            }

            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            StoryQueryFilter sqf = StoryQueryFilter.GetByID(id.Value, true, login.GetListAccessPermissionsForCurrentLogin());
            var story = _context.GetStoriesByFilter(sqf).SingleOrDefault();

            if (story == null)
            {
                return this.NotFound();
            }

            var viewModel = StoryBackendViewModel.CreateFromStory(story);
            viewModel.Login = login;
            this.ViewBag.RedirectTo = redirectTo;
            return this.View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid? id, string redirectTo)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.NotFound();
            }

            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            StoryQueryFilter sqf = StoryQueryFilter.GetByID(id.Value, true, login.GetListAccessPermissionsForCurrentLogin());
            var story = _context.GetStoriesByFilter(sqf).SingleOrDefault();
            if (story == null)
            {
                return this.NotFound();
            }
            _context.DeleteStory(story, this._blobService);
            _context.SaveChanges();
            this.TempData["Info"] = "Story deleted!";

            if (!string.IsNullOrEmpty(redirectTo))
            {
                return this.Redirect(redirectTo);
            }
            return this.RedirectToAction("Index", "Stories", new { area = "Backend" });
        }

        // GET: StoryEditor/Edit/5
        public IActionResult Edit(Guid? id, string redirectTo)
        {
            if (id == null || id.Value == Guid.Empty)
            {
                return this.NotFound();
            }

            var item = _context.GetStoriesByFilter(StoryQueryFilter.GetByID(id.Value, true)).SingleOrDefault();

            if (item == null)
            {
                return this.NotFound();
            }

            var viewModel = StoryBackendViewModel.CreateFromStory(item);
            viewModel.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);

            this.ViewData.CreateAccessLevelSelectList("ListAccess", viewModel.ListAccess);
            this.ViewData.CreateStoryStatusSelectList("Status", viewModel.Status);

            this.ViewData.CreateStoryCoverImageSelectList("CoverImage", item);

            string redirect = this.Url.GetStoryUrl(id.Value);

            if (!string.IsNullOrEmpty(redirectTo))
            {
                redirect = redirectTo;
            }

            this.ViewBag.RedirectTo = redirect;
            return this.View(viewModel);
        }

        // POST: StoryEditor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind()] StoryBackendViewModel viewModel, string redirectTo)
        {
            if (id != viewModel.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.GetStoriesByFilter(StoryQueryFilter.GetByID(viewModel.ID, false)).SingleOrDefault();
                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    viewModel.ApplyChangesTo(loaded);
                    _context.Update(loaded);
                    loaded.ModifiedUtc = DateTime.UtcNow;
                    loaded.ModifiedByUser = this.User?.Identity.Name;
                    await _context.SaveChangesAsync();
                    viewModel = StoryBackendViewModel.CreateFromStory(loaded);
                    viewModel.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.HasAnyStory(StoryQueryFilter.GetByID(viewModel.ID, false)))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return this.RedirectToAction(nameof(Index));
            }

            this.ViewData.CreateAccessLevelSelectList("ListAccess", viewModel.ListAccess);
            this.ViewData.CreateStoryStatusSelectList("Status", viewModel.Status);

            string redirect = this.Url.GetStoryUrl(viewModel.ID);

            if (!string.IsNullOrEmpty(redirectTo))
            {
                redirect = redirectTo;
            }

            this.ViewBag.RedirectTo = redirect;
            return this.View(viewModel);
        }

        public StoryEditorController(TrailContext context, BlobService blobService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public IActionResult RemoveBlock(Guid id, Guid storyid)
        {
            var block = _context.StoryBlocks.Include(sb => sb.Image).SingleOrDefault(sb => sb.ID == id);

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
    }
}
