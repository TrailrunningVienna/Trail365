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
    public class TrailNewsController : Controller
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

            var includeImages = false;
            var includeGpxBlob = false;
            var includePlaces = true;
            model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);

            var trails = _context.GetTrailsByListAccessOrderByDateDescending(includeImages, includeGpxBlob,includePlaces, model.Login.GetListAccessPermissionsForCurrentLogin(), _settings.MaxResultSize, _cache, _settings.AbsoluteExpirationInSecondsRelativeToNow);

            //Blob[] imagesList = new Blob[] { };

            //if (includeImages)
            //{
            //    imagesList = _context.GetRelatedPreviewImages(trails.ToArray());
            //}

            List<TrailNewsViewModel> list = new List<TrailNewsViewModel>();

            model.Items = trails.Select(t => t.ToTrailNewsViewModel(model.Login)).ToList();

           // model.Items.ForEach(tvm =>
           //{
           //    tvm.ShowDownloadLink = false;
           //    tvm.ShowEditLink = false;
           //});

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

        public TrailNewsController(TrailContext context,
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

       

      
    }
}
