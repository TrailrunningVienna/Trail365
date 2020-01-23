using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trail365.Data;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    public class TrailDetailsController : Controller
    {
        private readonly TrailContext _context;

        public TrailDetailsController(TrailContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TrailViewModel InitTrailViewModel(Guid trailID)
        {
            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            var filter = new TrailQueryFilter(login.GetListAccessPermissionsForCurrentLogin())
            {
                TrailID = trailID,
                IncludeGpxBlob = true,
                IncludePlaces = true,
                IncludeImages = false, //ols system is working!
            };

            var tr = _context.GetTrailsByFilter(filter).SingleOrDefault();

            if (tr == null)
            {
                throw new InvalidOperationException($"Trail with ID '{trailID}' does not exist.");
            }
            return tr.ToTrailViewModel(login, _context.GetRelatedPreviewImages(tr));

        }

        public IActionResult Trail(TrailRequestViewModel requestModel)
        {
            if (requestModel.ID.HasValue == false) return base.BadRequest();
            var model = this.InitTrailViewModel(requestModel.ID.Value);
            if (requestModel.NoConsent.HasValue)
            {
                model.NoConsent = requestModel.NoConsent.Value;
            }
            if (requestModel.Scraping.HasValue)
            {
                model.Scraping = requestModel.Scraping.Value;
            }
            return this.View("Trail", model);
        }
    }
}
