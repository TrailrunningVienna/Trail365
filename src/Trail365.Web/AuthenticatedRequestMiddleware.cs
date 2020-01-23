using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.Web
{
    /// <summary>
    /// Specialized Middleware, used only inside TestServer(xUnit)
    /// It creates the same (static) Claims-Principal Data for each request!
    /// </summary>
    public class AuthenticatedRequestMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public AuthenticatedRequestMiddleware(RequestDelegate next, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        private static readonly string[] RoleSeparators = new string[] { ";", "|", "," };

        public static ClaimsPrincipal CreateClaimsPrincipal(StaticUserSettings user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            List<Claim> claims = new List<Claim>
            {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.NameIdentifier, user.NameIdentifier),
                    new Claim(ClaimTypes.Email, user.EMail),
                    new Claim(ClaimTypes.PrimarySid, user.UserID.ToString())
            };

            var roles = string.Format("{0}", user.Roles).Trim().Split(RoleSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string role in roles)
            {
                if (!Enum.TryParse<Role>(role, out _))
                {
                    throw new InvalidOperationException($"Invalid Role name:'{role}'");
                }

                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "StaticAuthentication");
            Guard.Assert(claimsIdentity.IsAuthenticated);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            return claimsPrincipal;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_optionsMonitor.CurrentValue.StaticUserSettings.ShouldNotLogin == false)
            {
                context.User = CreateClaimsPrincipal(_optionsMonitor.CurrentValue.StaticUserSettings);
            }
            await _next(context);
        }
    }
}
