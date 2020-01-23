using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;

namespace Trail365.Web
{
    public static class ControllerExtension
    {
        public static void AuthenticateAs(this Controller controller, string userEmail, Role userRole)
        {
            AuthenticateAs(controller, userEmail, userRole.ToRoleList());
        }

        public static void AuthenticateAs(this Controller controller, string userEmail, string[] roles = null)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            ControllerContext cntx = new ControllerContext();
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEmail)
            };

            if (roles != null)
            {
                foreach (string role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var newClaimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var newPrincipal = new ClaimsPrincipal(newClaimsIdentity);
            cntx.HttpContext = new DefaultHttpContext() { User = newPrincipal };
            controller.ControllerContext = cntx;
        }
    }
}
