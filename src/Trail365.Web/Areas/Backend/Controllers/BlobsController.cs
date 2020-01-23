using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Backend.Controllers
{
    [Area("Backend")]
    [Route("[area]/[controller]/[action]")]
    [Authorize(Roles = BackendSetup.Roles)]
    public partial class BlobsController : Controller
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly AppSettings _settings;

        public BlobsController(TrailContext context, BlobService blobService, IOptionsMonitor<AppSettings> settingsMonitor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        // GET: Backend/Blobs
        public IActionResult Index(BlobsBackendIndexViewModel model)
        {
            if (model == null)
            {
                model = new BlobsBackendIndexViewModel();
            }

            model.PageController = "Blobs";
            model.PageSize = _settings.PageSize;
            model.EnablePaging = true;
            model.PageAction = nameof(Index);

            IQueryable<Blob> rawQuery = _context.Blobs;

            var data = rawQuery.OrderBy(p => p.Url);

            if (model.EnablePaging)
            {
                using (var tracker = _context.DependencyTracker("BlobsController.Index.Count"))
                {
                    model.UnpagedResults = data.Count();
                }
            }

            using (var tracker = _context.DependencyTracker("BlobsController.Index.Query"))
            {
                model.Blobs = data.Skip(model.SkipEntries).Take(model.PageSize).ToList();
            }

            model.Page = Math.Max(model.Page, 1);

            return this.View(model);
        }

        public BlobBackendViewModel InitModel(Guid id, bool includeReferences)
        {
            var item = _context.Blobs.SingleOrDefault(ev => ev.ID == id);
            if (item == null) return null;
            var vm = BlobBackendViewModel.CreateFromBlob(item);

            if (includeReferences)
            {
                //vm.References = _context.GetPlaceReferences(id, this.Url);
            }
            return vm;
        }

        // GET: Backend/Blobs/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null || id.Value == Guid.Empty)
            {
                return this.NotFound();
            }

            var model = this.InitModel(id.Value, false);

            return this.View(model);
        }

        // POST: Backend/Blobs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind()] BlobBackendViewModel viewModel)
        {
            if (id != viewModel.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.Blobs.Find(viewModel.ID);
                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    viewModel.ApplyChangesTo(loaded);
                    loaded.ModifiedUtc = DateTime.UtcNow;
                    loaded.ModifiedByUser = System.Threading.Thread.CurrentPrincipal?.Identity.Name;
                    _context.Update(loaded);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.BlobExists(viewModel.ID))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return this.RedirectToAction(nameof(Index));
            }
            return this.View(viewModel);
        }

        // GET: Backend/Blobs/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var model = this.InitModel(id.Value, true);

            return this.View(model);
        }

        // POST: Backend/Blobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var item = _context.Blobs.FirstOrDefault(e => e.ID == id);
            if (item == null)
            {
                return base.NotFound();
            }
            _context.DeleteBlob(item, _blobService);
            _context.SaveChanges();
            this.ViewBag.Info = $"Blob {item.Url} deleted with success (no references found).";
            return this.RedirectToAction(nameof(Index));
        }

        private bool BlobExists(Guid id)
        {
            return _context.Blobs.Any(e => e.ID == id);
        }
    }
}
