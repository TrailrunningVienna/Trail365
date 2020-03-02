using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    public class StoryController : Controller
    {
        private readonly TrailContext _context;
        private readonly IMemoryCache _cache;
        private readonly AppSettings _settings;
        public StoryViewModel InitStoryViewModel(Guid storyID, bool ignoreCache)
        {
            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            StoryQueryFilter sqf = StoryQueryFilter.GetByID(storyID, true, login.GetListAccessPermissionsForCurrentLogin());

            if (!ignoreCache)
            {
                sqf.Cache = _cache;
                sqf.AbsoluteExpiration = TimeSpan.FromSeconds(_settings.AbsoluteExpirationInSecondsRelativeToNow);
            }

            var story = _context.GetStoriesByFilter(sqf).SingleOrDefault();

            if (story == null)
            {
                throw new InvalidOperationException($"Story with ID '{storyID}' does not exist.");
            }

            return story.ToStoryViewModel(login);
        }

        public IActionResult Index(StoryRequestViewModel requestModel)
        {
            if (requestModel.ID.HasValue == false) return base.BadRequest();

            var model = this.InitStoryViewModel(requestModel.ID.Value, requestModel.IgnoreCache ?? false);

            if (requestModel.Scraping.HasValue)
            {
                model.Scraping = requestModel.Scraping.Value;
            }

            if (requestModel.NoConsent.HasValue)
            {
                model.NoConsent = requestModel.NoConsent.Value;
            }

            return this.View(model);
        }

        public StoryController(TrailContext context, IMemoryCache cache, IOptionsMonitor<AppSettings> settingsMonitor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _settings = settingsMonitor.CurrentValue;
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
