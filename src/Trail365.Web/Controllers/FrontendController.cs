using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trail365.Data;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    [Authorize(Roles = "Admin,User,Member,Moderator")]
    public class FrontendController : Controller
    {
        private readonly TrailContext _context;
        private readonly IdentityContext _identityContext;
        private readonly BlobService _blobService;

        public FrontendController(TrailContext context, IdentityContext identityContext, BlobService blobService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _identityContext = identityContext ?? throw new ArgumentException(nameof(identityContext));
        }

        [Route("frontend/index")]
        public IActionResult Index()
        {
            return this.RedirectToAction("Profile");
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

        private ProfileSettingsViewModel InitProfileSettingsViewModel(LoginViewModel login = null)
        {
            if (login == null)
            {
                if (this.User == null) throw new InvalidOperationException("No User assigned to this Controller/Context");
                login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            }
            if (!login.UserID.HasValue)
            {
                throw new InvalidOperationException("Something is wrong with the Claims-Transform");
            }
            var user = _identityContext.Users.Find(login.UserID.Value);
            this.ViewData["Login"] = login;
            return ProfileSettingsViewModel.CreateFromUser(user, login);
        }

        [HttpGet]
        public IActionResult ProfileSettings()
        {
            var model = this.InitProfileSettingsViewModel();
            return this.View(model);
        }

        [Route("frontend/profile")]
        [HttpGet]
        public IActionResult Profile()
        {
            var model = this.InitProfileSettingsViewModel();
            return this.View(model);
        }

        private bool UserExists(Guid id)
        {
            return _identityContext.Users.Any(e => e.ID == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProfileSettings(Guid id, ProfileSettingsViewModel model)
        {
            if (id != model.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _identityContext.Users.Find(model.ID);
                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    model.ApplyChangesTo(loaded);
                    _identityContext.Update(loaded);
                    _identityContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.UserExists(model.ID))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                model = this.InitProfileSettingsViewModel();
                return this.View(model);
            }
            return this.View(model);
        }
    }
}
