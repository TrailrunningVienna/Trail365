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
using Trail365.ViewModels;

namespace Trail365.Web.Backend.Controllers
{
    [Area("Backend")]
    [Route("[area]/[controller]/[action]")]
    [Authorize(Roles = BackendSetup.Roles)]
    public partial class PlacesController : Controller
    {
        private readonly TrailContext _context;

        private readonly AppSettings _settings;

        public PlacesController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
        }

        // GET: Backend/Places
        public IActionResult Index(PlacesBackendIndexViewModel model)
        {
            if (model == null)
            {
                model = new PlacesBackendIndexViewModel();
            }

            model.PageController = "Places";
            model.PageSize = _settings.PageSize;
            model.EnablePaging = true;
            model.PageAction = nameof(Index);

            IQueryable<Place> rawQuery = _context.Places;

            if (string.IsNullOrEmpty(model.SearchText) == false)
            {
                var splits = model.SearchText.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var searchtextpart in splits)
                {
                    rawQuery = rawQuery.Where(e => e.Name.ToLower().Contains(searchtextpart));
                }
            }

            if (string.IsNullOrEmpty(model.CountryTwoLetterISOCode) == false)
            {
                rawQuery = rawQuery.Where(p => p.CountryTwoLetterISOCode == model.CountryTwoLetterISOCode.ToUpperInvariant().Trim());
            }

            if (string.IsNullOrEmpty(model.ExternalSource) == false)
            {
                rawQuery = rawQuery.Where(p => p.ExternalSource.StartsWith(model.ExternalSource));
            }

            if (string.IsNullOrEmpty(model.ExternalID) == false)
            {
                rawQuery = rawQuery.Where(p => p.ExternalID.StartsWith(model.ExternalID));
            }

            var data = rawQuery.OrderBy(p => p.Name);

            if (model.EnablePaging)
            {
                _context.LogDependency("PlacesController.Index.Count", (op) => model.UnpagedResults = data.Count());
            }
            Place[] items = null;

            _context.LogDependency("PlacesController.Index.Query", (op) => items = data.Skip(model.SkipEntries).Take(model.PageSize).ToArray());
            Guard.AssertNotNull(items);
            model.Places = items.Select(t => PlaceBackendViewModel.CreateFromPlace(t)).ToList();

            model.Page = Math.Max(model.Page, 1);

            return this.View(model);
        }

        public PlaceBackendViewModel InitModel(Guid id, bool includeReferences)
        {
            var item = _context.Places.SingleOrDefault(ev => ev.ID == id);
            if (item == null) return null;
            var vm = PlaceBackendViewModel.CreateFromPlace(item);

            if (includeReferences)
            {
                vm.References = _context.GetPlaceReferences(id, this.Url);
            }

            return vm;
        }

        // GET: Backend/Places/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var model = this.InitModel(id.Value, false);

            if (id == null)
            {
                return this.NotFound();
            }

            this.ViewData.AddTwoLetterIsoCountryCodeLookupValues("TwoLetterIsoCodes", model.CountryTwoLetterISOCode);
            return this.View(model);
        }

        // POST: Backend/Places/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind()] PlaceBackendViewModel viewModel)
        {
            if (id != viewModel.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.Places.Find(viewModel.ID);
                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    viewModel.ApplyChangesTo(loaded);
                    loaded.ModifiedUtc = DateTime.UtcNow;
                    loaded.ModifiedByUser = this.User.Identity?.Name;
                    _context.Update(loaded);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.PlaceExists(viewModel.ID))
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

            this.ViewData.AddTwoLetterIsoCountryCodeLookupValues("TwoLetterIsoCodes", viewModel.CountryTwoLetterISOCode);

            return this.View(viewModel);
        }

        // GET: Backend/Places/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var model = this.InitModel(id.Value, true);

            if (id == null)
            {
                return this.NotFound();
            }

            this.ViewData.AddTwoLetterIsoCountryCodeLookupValues("TwoLetterIsoCodes", model.CountryTwoLetterISOCode);
            return this.View(model);
        }

        // POST: Backend/Places/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var item = _context.Places.FirstOrDefault(e => e.ID == id);
            if (item == null)
            {
                return base.NotFound();
            }
            _context.DeletePlace(item);
            _context.SaveChanges();
            this.ViewBag.Info = $"Place {item.Name} deleted with success (no references found).";
            return this.RedirectToAction(nameof(Index));
        }

        private bool PlaceExists(Guid id)
        {
            return _context.Places.Any(e => e.ID == id);
        }
    }
}
