using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public partial class StoriesController : Controller
    {
        private readonly TrailContext _context;

        private readonly AppSettings _settings;

        public StoriesController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
        }

        /// <summary>
        /// Get: backend/stories
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(StoriesBackendIndexViewModel model)
        {
            if (model == null)
            {
                model = new StoriesBackendIndexViewModel();
            }

            model.PageController = "Stories";

            model.PageSize = _settings.PageSize;

            model.EnablePaging = true;
            model.PageAction = nameof(Index);

            StoryQueryFilter qf = new StoryQueryFilter
            {
                OrderBy = StoryQueryOrdering.DescendingName,
                IncludeBlocks = true,
                SearchText = model.SearchText
            };

            if (model.Status.HasValue)
            {
                qf.IncludedStatus = new StoryStatus[] { model.Status.Value };
            }

            if (model.EnablePaging)
            {
                model.UnpagedResults = _context.GetStoryCount(qf);
            }

            qf.Take = model.PageSize;
            qf.Skip = model.SkipEntries;
            var items = _context.GetStoriesByFilterQueryable(qf);
            Guard.AssertNotNull(items);
            this.ViewData.CreateStoryStatusSelectList("FilterStatus", model.Status);
            model.Stories = items.Select(s => StoryBackendViewModel.CreateFromStory(s)).ToList();
            model.Page = Math.Max(model.Page, 1);
            return this.View(model);
        }
    }
}
