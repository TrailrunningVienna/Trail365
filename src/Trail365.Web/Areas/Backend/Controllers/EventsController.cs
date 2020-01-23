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
using Trail365.Internal;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Backend.Controllers
{
    [Area("Backend")]
    [Route("[area]/[controller]/[action]")]
    [Authorize(Roles = BackendSetup.Roles)]
    public partial class EventsController : Controller
    {
        private readonly TrailContext _context;

        private readonly BlobService _blobService;

        private readonly AppSettings _settings;

        public EventsController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor, BlobService blobService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
        }

        public IActionResult Index(EventBackendIndexViewModel model)
        {
            if (model == null)
            {
                model = new EventBackendIndexViewModel();
            }

            model.PageController = "Events";

            model.PageSize = _settings.PageSize;

            model.EnablePaging = true;
            model.PageAction = nameof(Index);

            //var skip = (page - 1) * limit;
            //Backend, don't restrict to permissions!

            IQueryable<Event> rawQuery = _context.Events.Include(e => e.Place);

            if (model.Status.HasValue)
            {
                rawQuery = rawQuery.Where(e => e.Status == model.Status.Value);
            }

            if (string.IsNullOrEmpty(model.SearchText) == false)
            {
                foreach (var searchtextpart in model.SearchText.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    rawQuery = rawQuery.Where(e => e.Name.ToLower().Contains(searchtextpart));
                }
            }
            if (model.From.HasValue && model.To.HasValue)
            {
                if (model.From.Value > model.To.Value)
                {
                    //invalid
                    model.To = null; //reset one restriction to ensure result and no exception!
                }
            }

            if (model.From.HasValue)
            {
                rawQuery = rawQuery.Where(e => e.StartTimeUtc >= model.From.Value.ToUniversalTime());
            }

            if (model.To.HasValue)
            {
                rawQuery = rawQuery.Where(e => e.EndTimeUtc <= model.To.Value.ToUniversalTime());
            }

            var data = rawQuery.OrderByDescending(e => e.StartTimeUtc);

            if (model.EnablePaging)
            {
                _context.LogDependency($"EventsController.Index.Count", op =>
                {
                    model.UnpagedResults = data.Count(); //execute the basequery WITHOUT skip/take but WITH count(*);
                });
            }

            Event[] items = null;
            _context.LogDependency("EventsController.Index.Query", (op) =>
            {
                items = data.Skip(model.SkipEntries).Take(model.PageSize).ToArray();
            });

            Guard.AssertNotNull(items);
            this.ViewData.CreateEventStatusSelectList("FilterStatus", model.Status);

            model.Events = items.Select(t => EventBackendViewModel.CreateFromEntity(t)).ToList();

            model.Page = Math.Max(model.Page, 1);

            return this.View(model);
        }

        // GET: Backend/Events/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);

            EventQueryFilter filter = new EventQueryFilter(login.GetListAccessPermissionsForCurrentLogin(), restrictToPublishedEventsOnly: false)
            {
                EventID = id,
                IncludePlaces = true,
                IncludeImages = true,
                IncludeTrails = false,
            };

            var item = _context.GetEventsByFilter(filter).SingleOrDefault();

            if (item == null)
            {
                return this.NotFound();
            }

            this.ViewData.CreateAccessLevelSelectList("ReadAccess", item.ListAccess);

            var placesLookupList = _context.GetPlacesForSelectList().ToArray();
            this.ViewData.AddPlacesLookupValues("PlaceID", item.PlaceID, placesLookupList);
            this.ViewData.AddPlacesLookupValues("EndPlaceID", item.EndPlaceID, placesLookupList);

            this.ViewData.AddTrailsLookupValues("TrailID", item.TrailID, _context.GetTrails(false, false).ToArray());
            this.ViewData.CreateEventStatusSelectList("Status", item.Status);
            return this.View(EventBackendViewModel.CreateFromEntity(item));
        }

        // POST: Backend/Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind()] EventBackendViewModel viewModel)
        {
            if (id != viewModel.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.Events.SingleOrDefault(e => e.ID == id);

                    if (loaded == null)
                    {
                        return base.NotFound();
                    }

                    viewModel.ApplyChangesTo(loaded);
                    _context.Update(loaded);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.EventExists(viewModel.ID))
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

            this.ViewData.CreateAccessLevelSelectList("ReadAccess", viewModel.ReadAccess);
            var placesLookupList = _context.GetPlacesForSelectList().ToArray();
            this.ViewData.AddPlacesLookupValues("PlaceID", viewModel.PlaceID, placesLookupList);
            this.ViewData.AddPlacesLookupValues("EndPlaceID", viewModel.EndPlaceID, placesLookupList);
            this.ViewData.AddTrailsLookupValues("TrailID", viewModel.TrailID, _context.GetTrails(false, false).ToArray());
            this.ViewData.CreateEventStatusSelectList("Status", viewModel.Status);

            return this.View(viewModel);
        }

        // GET: Backend/Events/Delete/5
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
            throw new NotImplementedException("Delete View");
            //return this.View(vm);
        }

        // POST: Backend/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var item = _context.Events.FirstOrDefault(t => t.ID == id);
            if (item == null)
            {
                return base.NotFound();
            }
            _context.DeleteEvent(item, _blobService);
            _context.SaveChanges();
            return this.RedirectToAction(nameof(Index));
        }

        private bool EventExists(Guid id)
        {
            return _context.Events.Any(e => e.ID == id);
        }
    }
}
