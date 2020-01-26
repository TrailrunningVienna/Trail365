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
    public class StoryNewsController : Controller
    {
        private readonly TrailContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _cache;
        public StoryNewsController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor, IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
            _cache = cache;
        }

        public StoryCollectionViewModel InitStoryCollectionViewModel(StoryCollectionViewModel model, LoginViewModel login, bool ignoreCache)
        {
            if (model == null)
            {
                model = new StoryCollectionViewModel();
            }

            if (login != null)
            {
                model.Login = login;
            }
            else
            {
                model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            }
            this.ViewData["Login"] = model.Login;

            StoryQueryFilter qf = new StoryQueryFilter(model.Login.GetListAccessPermissionsForCurrentLogin(), true)
            {
                Take = _settings.MaxResultSize,
                OrderBy = StoryQueryOrdering.DescendingCreationOrModificationDate,
                SearchText = model.SearchText,
                IncludeBlocks = true
            };

            if (!ignoreCache)
            {
                qf.Cache = _cache;
                qf.AbsoluteExpiration = TimeSpan.FromSeconds(_settings.AbsoluteExpirationInSecondsRelativeToNow);
            }

            var baseList = _context.GetStoriesByFilter(qf);

            model.Stories = baseList.Select(e =>
            {
                var tvm = e.ToStoryViewModel(model.Login);
                return tvm;
            }).ToList();
            return model;
        }

        public IActionResult Index(NewsRequestViewModel requestModel)
        {
            var model = this.InitStoryCollectionViewModel(null, null, requestModel.IgnoreCache ?? false);
            return this.View(model);
        }
    }
}
