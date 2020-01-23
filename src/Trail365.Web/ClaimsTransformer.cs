using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.Web
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IdentityContext _context;
        private readonly IOptionsMonitor<AppSettings> _settings;
        private readonly ILogger<ClaimsTransformer> _logger;

        public ClaimsTransformer(IdentityContext context, IOptionsMonitor<AppSettings> settings, ILogger<ClaimsTransformer> logger)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this._logger = logger ?? throw new ArgumentNullException(nameof(settings));
        }

        private static int AppendOrIgnoreRoles(List<Claim> list, params string[] roleNames)
        {
            Guard.Assert(list != null);
            Guard.Assert(roleNames != null);

            var items = roleNames.Select<string, Claim>(roleName => new Claim(ClaimTypes.Role, roleName)).ToArray();

            int added = 0;
            foreach (var item in items)
            {
                bool exists = false;
                foreach (var existent in list)
                {
                    if (existent.Type != item.Type) continue;
                    if (existent.Value != item.Value) continue;
                    exists = true;
                }
                if (exists) continue;
                list.Add(item);
                added += 1;
            }
            return added;
        }

        //https://stackoverflow.com/questions/51888859/asp-net-core-2-1-register-custom-claimsprincipal
        Task<ClaimsPrincipal> IClaimsTransformation.TransformAsync(ClaimsPrincipal principal)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                Guard.Assert(principal != null);
                Guard.Assert(principal.Identity != null);
                Guard.Assert(principal.Identities.Count() <= 1);

                List<Claim> claims = new List<Claim>();
                ClaimsIdentity newClaimsIdentity;
                ClaimsPrincipal newPrincipal;

                claims = new List<Claim>(principal.Claims);

                if (principal == null) throw new ArgumentNullException(nameof(principal));

                bool rolesForPrincipalFound = _context.TryGetRoles(principal, out var roleNames, out var userID);

                if (rolesForPrincipalFound)
                {
                    var psidClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid);
                    if (psidClaim == null)
                    {
                        claims.Add(new Claim(ClaimTypes.PrimarySid, userID.ToString()));
                    }
                }

                if (principal.IsSuperAdmin(_settings.CurrentValue.AdminUsers))
                {
                    AppendOrIgnoreRoles(claims, Role.Admin.ToRoleList());
                    newClaimsIdentity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType);
                }
                else
                {
                    if (rolesForPrincipalFound == false)
                    {
                        //usecase: first contact, we don't have any information/user entry
                        newClaimsIdentity = new ClaimsIdentity();
                        Guard.Assert(newClaimsIdentity.IsAuthenticated == false);
                    }
                    else
                    {
                        AppendOrIgnoreRoles(claims, roleNames);
                        newClaimsIdentity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType);
                    }
                }

                newPrincipal = new ClaimsPrincipal(newClaimsIdentity);
                return Task.FromResult(newPrincipal);
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex, "ClaimsTransformer-Issue");
                throw;
            }
            finally
            {
                sw.Stop();
                this._logger.LogTrace($"TransformAsync: Duration={sw.ElapsedMilliseconds} msec");
            }
        }
    }
}
