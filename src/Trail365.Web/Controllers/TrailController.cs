using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;
using Trail365.Tasks;
using Trail365.ViewModels;
using Trail365.Web.Tasks;

namespace Trail365.Web.Controllers
{
    public class TrailController : Controller
    {
        private readonly TrailContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _cache;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TrailCollectionViewModel InitTrailCollectionViewModel(TrailCollectionViewModel model)
        {
            if (model == null)
            {
                model = new TrailCollectionViewModel();
            }

            var includeImages = true;
            var includeGpxBlob = true;

            model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);

            var trails = _context.GetTrailsByListAccessOrderByDateDescending(includeImages, includeGpxBlob, model.Login.GetListAccessPermissionsForCurrentLogin(), _settings.MaxResultSize, _cache, _settings.AbsoluteExpirationInSecondsRelativeToNow);

            Blob[] imagesList = new Blob[] { };

            if (includeImages)
            {
                imagesList = _context.GetRelatedPreviewImages(trails.ToArray());
            }

            List<TrailViewModel> list = new List<TrailViewModel>();

            model.Items = trails.Select(t => t.ToTrailViewModel(model.Login, !includeImages, imagesList)).ToList();

            model.Items.ForEach(tvm =>
           {
               tvm.ShowDownloadLink = false;
               tvm.ShowEditLink = false;
           });
            return model;
        }

        public IActionResult Index(NewsRequestViewModel requestModel)
        {
            var model = this.InitTrailCollectionViewModel(null);
            return this.View(model);
        }

        private CreateTrailViewModel InitCreateTrailViewModel(CreateTrailViewModel model = null)
        {
            if (model == null)
            {
                model = new CreateTrailViewModel();
            }
            model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            return model;
        }

        public TrailController(TrailContext context,
                               BlobService blobService, ILogger<TrailController> logger, IMemoryCache cache, IOptionsMonitor<AppSettings> settingsMonitor, IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _settings = settingsMonitor.CurrentValue;
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

        }

        /// <summary>
        /// Http-Get
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User,Member,Moderator")]
        [HttpGet]
        public IActionResult CreateTrail()
        {
            var model = this.InitCreateTrailViewModel(null);
            return this.View(model);
        }

        /// <summary>
        /// Http-Post at the end of "MultiGpxUpload"
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User,Member,Moderator")]
        [HttpPost]
        public IActionResult SaveGpx(CreateTrailViewModel model)
        {
            if (model == null) return this.NotFound();
            Trail trail = null;

            using (var tracker = _context.DependencyTracker(nameof(SaveGpx)))
            {
                trail = _context.Trails.Where(t => t.ID == model.ID).SingleOrDefault();
            }

            if (trail == null) return this.NotFound();

            _context.SaveChanges();
            if (!_settings.BackgroundServiceDisabled)
            {
                var task = BackgroundTaskFactory.CreateTask<TrailPreviewTask>(this._serviceScopeFactory, this.Url);
                task.Trail = trail;
                task.Queue(this._queue);
                //TempData empty during test!
                if (this.TempData != null)
                {
                    this.TempData["Info"] = $"Vorschau Berechnung für '{trail.Name}' wurde im Hintergrund gestartet! Das Ergebnis sollte in wenigen Minuten für alle User sichtbar sein.";
                }
            }
            else
            {
                //TempData empty during test!
                if (this.TempData != null)
                {
                    this.TempData["Info"] = $"Vorschau Berechnung für '{trail.Name}' wurde NICHT im Hintergrund gestartet, da Hintergrundtätigkeiten derzeit deaktiviert sind.";
                }
            }
            return this.RedirectToAction("Trail", "TrailDetails", new { trail.ID });
        }

        [Authorize(Roles = "Admin,User,Member,Moderator")]
        public IActionResult UploadGpx(Guid id, IFormFile file)
        {
            var model = this.InitCreateTrailViewModel();
            model.ID = id;

            Trail existing = null;
            using (var tracker = _context.DependencyTracker(nameof(UploadGpx)))
            {
                existing = _context.Trails.Include(t => t.GpxBlob).Where(t => t.ID == id).SingleOrDefault();
            }

            if (existing == null)
            {
                existing = new Trail
                {
                    ID = id
                };
                existing.GpxBlob = new Blob();
                existing.GpxBlobID = existing.GpxBlob.ID;
                _context.Trails.Add(existing);
                _context.Blobs.Add(existing.GpxBlob);
            }
            else
            {
                Guard.AssertNotNull(existing.GpxBlob);
                _context.Blobs.Update(existing.GpxBlob);
            }

            if (model.GpxID.HasValue == false)
            {
                model.GpxID = existing.GpxBlobID;
            }

            using (var stream = file.OpenReadStream())
            {
                var mapping = _blobService.UploadStream(stream, model.GpxID.Value, "gpx", "gpx", this.Url);
                mapping.ApplyToBlob(existing.GpxBlob);
                existing.GpxBlob.OriginalFileName = file.FileName;
                stream.Position = 0;
                TrailExtender.ReadGpxFileInfo(stream.ToBytes(), existing);

                if (string.IsNullOrEmpty(existing.Name))
                {
                    existing.Name = file.Name;
                }

                if (string.IsNullOrEmpty(existing.Name))
                {
                    existing.Name = existing.ID.ToString();
                }
            }
            _context.SaveChanges();
            model.FileUrl = existing.GpxBlob.Url;
            model.Name = existing.Name;
            return this.PartialView("_TrailUpload", model);
        }
    }
}
