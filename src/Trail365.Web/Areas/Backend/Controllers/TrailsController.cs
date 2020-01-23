using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Backend.Controllers
{
    [Area("Backend")]
    [Authorize(Roles = BackendSetup.Roles)]
    public partial class TrailsController : Controller
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly IOptionsMonitor<AppSettings> _settingsMonitor;

        public TrailsController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor, BlobService blobService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
        }

        // GET: Backend/Trails
        public IActionResult Index(int page = 1, string searchtext = null)
        {
            TrailQueryFilter qf = new TrailQueryFilter();
            var limit = _settingsMonitor.CurrentValue.MaxResultSize;
            qf.Skip = (page - 1) * limit;
            qf.Take = limit;
            qf.SearchText = searchtext;
            qf.OrderBy = TrailQueryOrdering.DescendingCreationOrModificationDate;

            var trails = _context.GetTrailsByFilter(qf).ToArray();
            //var images = _context.GetRelatedPreviewImages(trails);
            this.ViewBag.Info = this.TempData["Info"];
            return this.View("Index", trails.Select(t => TrailBackendViewModel.CreateFromTrail(t)).ToList());
        }

        [HttpGet]
        public IActionResult Edit(Guid? id)
        {
            if (id.HasValue == false || id == Guid.Empty)
            {
                return this.NotFound();
            }

            var trail = _context.GetTrailsByFilter(TrailQueryFilter.GetByID(id.Value, true)).SingleOrDefault();

            if (trail == null)
            {
                return this.NotFound();
            }

            this.ViewData.CreateAccessLevelSelectList("ReadAccess", trail.ListAccess);
            this.ViewData.CreateAccessLevelSelectList("DownloadAccess", trail.GpxDownloadAccess);
            this.ViewData.AddPlacesLookupValues("StartPlaceID", trail.StartPlaceID, _context.GetPlacesForSelectList().ToArray());
            this.ViewData.AddPlacesLookupValues("EndPlaceID", trail.EndPlaceID, _context.GetPlacesForSelectList().ToArray());

            return this.View(TrailBackendViewModel.CreateFromTrail(trail, _context.GetRelatedPreviewImages(trail)));
        }

        // POST: Backend/Trails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind()] TrailBackendViewModel trail)
        {
            if (id != trail.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.Trails.Find(trail.ID);
                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    trail.ApplyChangesTo(loaded);
                    _context.Update(loaded);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TrailExists(trail.ID))
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

            this.ViewData.CreateAccessLevelSelectList("ReadAccess", trail.ReadAccess);
            this.ViewData.CreateAccessLevelSelectList("DownloadAccess", trail.DownloadAccess);
            this.ViewData.AddPlacesLookupValues("StartPlaceID", trail.StartPlaceID, _context.GetPlacesForSelectList().ToArray());
            this.ViewData.AddPlacesLookupValues("EndPlaceID", trail.EndPlaceID, _context.GetPlacesForSelectList().ToArray());

            return this.View(trail);
        }

        // GET: Backend/Trails/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var trail = await _context.Trails.FirstOrDefaultAsync(t => t.ID == id);
            if (trail == null)
            {
                return this.NotFound();
            }

            var images = _context.GetRelatedPreviewImages(trail);

            TrailBackendViewModel vm = TrailBackendViewModel.CreateFromTrail(trail, images);

            return this.View(vm);
        }

        // POST: Backend/Trails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var trail = _context.Trails.FirstOrDefault(t => t.ID == id); //we DON'T need includes here because we check the ID's for each subProperty (Deleting trail is a complex process)!

            if (trail == null)
            {
                return base.NotFound();
            }
            _context.DeleteTrail(trail, _blobService);
            _context.SaveChanges();
            return this.RedirectToAction(nameof(Index));
        }
    }
}
