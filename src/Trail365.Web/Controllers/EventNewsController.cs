using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
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

        public NewsCollectionViewModel InitNewsCollectionViewModel(bool scraping)
        {
            var model = new NewsCollectionViewModel
            {
                Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User)
            };

            if (scraping)
            {
                model.Scraping = true;
                return model;
            }

            Features features = _settings.Features;
            List<Tuple<Event, int>> fullEventList;

            fullEventList = _context.GetEventStreamForNews(_settings.MaxPromotionSize, _settings.MaxResultSize, DateTime.UtcNow, _cache, model.Login.GetListAccessPermissionsForCurrentLogin(), _settings.ResponseCacheDurationSeconds).ToList().ToList();

            Guard.Assert(fullEventList.Select(n => n.Item1.ID).Distinct().Count() == fullEventList.Count, "distinct alarm 3");

            List<NewsViewModel> list = new List<NewsViewModel>();

            var eventNews = this.Url.Transform(model.Login, fullEventList).ToList();
            eventNews.ForEach(evm =>
            {
                Guard.AssertNotNull(evm.EventItem);
                if (evm.EventItem.Place != null)
                {
                    evm.EventItem.Place.ShowEditLink = false;
                }
                if (evm.EventItem.EndPlace != null)
                {
                    evm.EventItem.EndPlace.ShowEditLink = false;
                }
                if (evm.EventItem.Trail != null)
                {
                    evm.EventItem.Trail.ShowDownloadLink = false;
                    evm.EventItem.Trail.ShowEditLink = false;
                }
            });

            list.AddRange(eventNews);

            model.News.Clear();

            model.News.AddRange(list.OrderByDescending(n => n.Priority).ThenByDescending(n => n.Time));

            Guard.Assert(list.Where(n => !n.OriginID.HasValue).Count() == 0, "originID expected");

            if (list.Select(n => n.OriginID.Value).Distinct().Count() != list.Count)
            {
                list.ForEach(n => System.Diagnostics.Debug.WriteLine(n.OriginID));
                //Guard.Assert(list.Select(n => n.OriginID.Value).Distinct().Count() == list.Count, "distinct alarm 2");
            }
            return model;
        }

        public IActionResult Index(NewsRequestViewModel requestModel)
        {
            var model = this.InitNewsCollectionViewModel(requestModel?.Scraping ?? false);
            model.NoConsent = requestModel?.NoConsent ?? false;
            return this.View("News", model);
        }
    }
}
