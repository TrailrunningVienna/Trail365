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
    public class EventNewsController : Controller
    {
        private readonly TrailContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _cache;

        public EventNewsController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor, IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }


        public EventCollectionViewModel InitEventCollectionViewModel(EventCollectionViewModel model = null, LoginViewModel login = null, Guid? restrictToOwner = null, bool includeTrails = false)
        {
            if (model == null)
            {
                model = new EventCollectionViewModel();
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

            EventQueryFilter filter = new EventQueryFilter(model.Login.GetListAccessPermissionsForCurrentLogin(), restrictToPublishedEventsOnly: true)
            {
                IncludePlaces = true,
                IncludeImages = false,
                IncludeTrails = includeTrails,
                Take = _settings.MaxResultSize,
                OrderBy = EventQueryOrdering.AscendingStartDate,
                OwnerID = restrictToOwner
            };

            //don't show the past on our news stream!
            filter.StartTimeUtcMinValue = DateTime.UtcNow.AddHours(-5);

            var eventList = _context.GetEventsByFilter(filter);

            if (model.HasSearchText())
            {
                throw new NotImplementedException("Searchtext");
            }

            model.Events = eventList.Select(e =>
            {
                bool hasTrailPermission = false;

                if (e.Trail != null)
                {
                    hasTrailPermission = model.Login.CanDo(e.Trail.ListAccess);
                }

                var tvm = e.ToEventViewModel(this.Url, model.Login, null, hasTrailPermission).EnableEditLinkForTrail().HideChallenge();
                return tvm;

            }).OrderBy(item => item.StartDate).ToList();

            return model;
        }

        public IActionResult Index(NewsRequestViewModel requestModel)
        {
            var model = this.InitEventCollectionViewModel(null, includeTrails: true);
            model.NoConsent = requestModel?.NoConsent ?? false;
            return this.View("Index", model);
        }
    }
}
