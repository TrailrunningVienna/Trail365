using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    //https://github.com/gdyrrahitis/bpost-aspnetcore-social-logins/blob/master/Social.Logins.Web/Views/Auth/SignIn.cshtml

    [Route(AuthenticationRoute.ControllerRouteName)]
    public class AuthController : Controller
    {
        private readonly AuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService as AuthenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        private async Task<DiagnosticsViewModel> CreateViewModel()
        {
            return new DiagnosticsViewModel(await this.HttpContext.AuthenticateAsync(), this._authenticationService);
        }

        [Route("diag")]
        public async Task<IActionResult> Diagnostics()
        {
            var viewModel = await this.CreateViewModel();
            if (this.HttpContext.Request.Headers.TryGetValue("ContentType", out var value))
            {
                if (value == "application/json")
                {
                    return this.Json(viewModel);
                }
            }
            return this.View(viewModel);
        }

        [Route("signin")]
        public IActionResult SignIn()
        {
            return this.View();
        }

        [Route("signin/{provider}")]
        public IActionResult SignIn(string provider, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) == false)
            {
                if (this.Url.IsLocalUrl(returnUrl) == false)
                {
                    throw new InvalidOperationException("ReturnUrl must be a local Url");
                }
            }
            switch (provider)
            {
                case "Standard":
                    throw new InvalidOperationException("not planned for this project");
                case "Google":
                case "GitHub":
                case "Facebook":
                    return this.Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" }, provider);
            }
            return base.BadRequest();
        }

        [Route("signout")]
        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return this.RedirectToAction("Index", "Home");
        }
    }
}
