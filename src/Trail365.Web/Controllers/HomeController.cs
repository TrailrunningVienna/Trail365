using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Tasks;
using Trail365.Tasks.Internal;
using Trail365.ViewModels;
using Trail365.Web.Tasks;

namespace Trail365.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly TrailContext _context;
        private readonly AppSettings _settings;
        private readonly ILogger _logger;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _cache;

        public HomeController(TrailContext context, IOptionsMonitor<AppSettings> settingsMonitor, ILogger<HomeController> logger, IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory, IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }


        public HomeViewModel InitModel(HomeViewModel model = null, LoginViewModel login = null, Guid? restrictToOwner = null)
        {
            if (model == null)
            {
                model = new HomeViewModel();
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

            TrailQueryFilter trailFilter = new TrailQueryFilter(model.Login.GetListAccessPermissionsForCurrentLogin())
            {
                Take = _settings.MaxResultSize,
                OwnerID = restrictToOwner,
                SearchText = model.SearchText,
            };

            var trailList = _context.GetTrailsByFilter(trailFilter);

            var imagesList = _context.GetRelatedPreviewImages(trailList.ToArray());

            model.IndexTrails = trailList.Select(t =>
            {
                var tvm = t.ToTrailViewModel(model.Login, imagesList);
                return tvm;
            }).ToList();
            this.ViewData.AddOverallChallengeOption(model.GetOverallChallengeLevel());
            return model;
        }

        public IActionResult AllTrails(HomeViewModel model)
        {
            model = this.InitModel(model);
            return this.View("Index", model);
        }

        [Authorize()]
        public IActionResult MyTrails(HomeViewModel model)
        {
            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            model = this.InitModel(model, login, login.UserID.Value);
            return this.View("Index", model);
        }

        public IActionResult Search(HomeViewModel model)
        {
            model = this.InitModel(model);
            this.ViewData.AddSearchFieldOption(true);
            return this.View("Index", model);
        }

        [HttpGet("layout/{key}")]
        public IActionResult Layout(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "default";
            }
            return this.View("Layout", new LayoutViewModel { Key = key });
        }

        public IActionResult Layout()
        {
            return this.View("Layout", new LayoutViewModel { Key = "default" });
        }

        public IActionResult Index(NewsRequestViewModel requestModel)
        {
            //this is our main/default entry point from outside.
            //the view showed here is depending on the enabled/disabled feature (the first new-stream feature that is enabled or a default view
            if (_settings.Features.Events)
            {
                return this.RedirectToAction("Index", "EventNews", requestModel);
            }

            if (_settings.Features.Trails)
            {
                return this.RedirectToAction("Index","TrailNews", requestModel);
            }

            if (_settings.Features.Stories)
            {
                return this.RedirectToAction("Index", "StoryNews", requestModel);
            }

            return this.View();
        }

        public EventViewModel InitEventViewModel(Guid? eventID, LoginViewModel login = null)
        {
            if (login == null)
            {
                login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            }

            if (eventID.HasValue)
            {
                if (eventID.Value == Guid.Empty)
                {
                    eventID = null;
                }
            }

            bool restrictToPublishedEventsOnly = !eventID.HasValue;
            EventQueryFilter filter = new EventQueryFilter(login.GetListAccessPermissionsForCurrentLogin(), restrictToPublishedEventsOnly)
            {
                IncludePlaces = true,
                IncludeImages = true,
                IncludeTrails = true,
                EventID = eventID
            };

            var resultEvent = _context.GetEventsByFilter(filter).FirstOrDefault();

            if (resultEvent == null)
            {
                throw new InvalidOperationException($"Event with ID '{eventID}' does not exist or is not permitted");
            }

            Blob[] images = null;
            bool hasTrailPermission = false;

            if (resultEvent.Trail != null)
            {
                hasTrailPermission = login.CanDo(resultEvent.Trail.ListAccess);
            }

            if (resultEvent.Trail != null && hasTrailPermission)
            {
                images = _context.GetRelatedPreviewImages(resultEvent.Trail);
            }

            var vm = resultEvent.ToEventViewModel(this.Url, login, images, hasTrailPermission).EnableEditLinkForPlaces().EnableDownloadLinkForTrail().EnableEditLinkForTrail();
            return vm;
        }

        public IActionResult Event(EventRequestViewModel requestModel)
        {
            if (requestModel.ID.HasValue == false) return base.BadRequest();
            //in case Event.Trail is available we need preview and elevation data
            var model = this.InitEventViewModel(requestModel.ID.Value);
            if (requestModel.NoConsent.HasValue)
            {
                model.NoConsent = requestModel.NoConsent.Value;
            }
            return this.View(model);
        }

        [HttpGet("demo")]
        public IActionResult StartBackgroundDemoSync()
        {
            BackgroundTaskFactory.CreateTask<LogTask>(this._serviceScopeFactory, this.Url, this._logger).Queue(this._queue);
            this.TempData["Info"] = $"Demo Backgroundtask wurde im Hintergrund gestartet! Das Ergebnis sollte in wenigen Sekunden im Log sichtbar werden.";
            return this.RedirectToAction(nameof(Index));
        }

        public IActionResult Impressum()
        {
            return this.View();
        }

        public IActionResult Datenschutz()
        {
            return this.View();
        }

        //    [Authorize]
        public ActionResult SendMessage(int userID, string message)
        {
           // UserManager UM = new UserManager();
           // UM.AddMessage(userID, message);
            return Json(new { success = true });
        }

    //    [Authorize]
        public ActionResult GetMessages()
        {
            List<UserMessage> l = new List<UserMessage>();
            l.Add(new UserMessage()
            {
               FirstName = "frist name 1",
               LastName = "last name 1",
               MessageID = 99,
               MessageText ="Hello world",
               LogDate = DateTime.Now,
               SYSUserID = 77,
            });
            //UserManager UM = new UserManager();
            //return Json(UM.GetAllMessages(), JsonRequestBehavior.AllowGet);
            return Json(l,null);

        }



    }
}
