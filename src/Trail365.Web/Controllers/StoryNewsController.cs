using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        public StoryNewsController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
        }

        public StoryCollectionViewModel InitStoryCollectionViewModel(StoryCollectionViewModel model = null, LoginViewModel login = null)
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
            var model = this.InitStoryCollectionViewModel();
            return this.View(model);
        }
    }
}
