using System;
using System.Collections.Generic;
using System.Security.Claims;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.ViewModels
{
    public class LoginViewModel
    {
        public bool IsModeratorOrHigher => this.IsModerator || this.IsAdmin;

        public bool IsModerator => this.IsLoggedIn && (this.UserRoles.HasFlag(Role.Moderator));

        public bool IsMember => this.IsLoggedIn && (this.UserRoles.HasFlag(Role.Member));

        public bool IsAdmin => this.IsLoggedIn && (this.UserRoles.HasFlag(Role.Admin));

        public bool IsLoggedIn { get; private set; }

        public LoginViewModel()
        {
            this.UserRoles = Role.None;
            Guard.Assert(this.IdentityID.HasValue == false);
            Guard.Assert(this.UserID.HasValue == false);
            Guard.Assert(this.IsLoggedIn == false);
        }

        public string[] GetAssignedRoleNames()
        {
            List<string> roles = new List<string>();
            foreach (Role rl in System.Enum.GetValues(typeof(Role)))
            {
                if (rl == Role.None) continue;
                if (this.UserRoles.HasFlag(rl))
                {
                    roles.Add(rl.ToDescription());
                }
            }
            return roles.ToArray();
        }

        public static LoginViewModel CreateFromClaimsPrincipalOrDefault(ClaimsPrincipal principalOrDefault)
        {
            var result = new LoginViewModel();
            if (principalOrDefault == null) return result;
            result.IsLoggedIn = principalOrDefault.Identity.IsAuthenticated;
            if (result.IsLoggedIn == false) return result;

            foreach (var cl in principalOrDefault.Claims)
            {
                if (cl.Type == ClaimTypes.Role)
                {
                    string v = cl.Value;
                    if (System.Enum.TryParse<Role>(v, out var r))
                    {
                        result.UserRoles |= r;
                    }
                }
                if (cl.Type == ClaimTypes.PrimarySid)
                {
                    string v = cl.Value;
                    if (System.Guid.TryParse(v, out var g))
                    {
                        result.UserID = g;
                    }
                }
            }
            return result;
        }

        public Guid? UserID { get; set; }
        public Guid? IdentityID { get; set; }
        public Role UserRoles { get; set; }

        public bool BackendAvailable
        {
            get
            {
                if (this.IsLoggedIn == false) return false;
                foreach (var r in BackendSetup.RolesAsEnum)
                {
                    if (this.UserRoles.HasFlag(r)) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 08/2019 here we implement permission Cascading for the moment!
        /// </summary>
        /// <returns></returns>
        public AccessLevel[] GetListAccessPermissionsForCurrentLogin()
        {
            if (this.IsLoggedIn == false)
            {
                return new AccessLevel[] { AccessLevel.Public };
            }

            if (this.IsAdmin)
            {
                return EntityExtension.AllAccessLevels;
            }
            List<AccessLevel> l = new List<AccessLevel>();
            Guard.Assert(this.IsLoggedIn);
            //add public and user for "login"!
            l.Add(AccessLevel.Public);
            l.Add(AccessLevel.User);

            if (this.IsModerator)
            {
                l.Add(AccessLevel.Moderator);
                l.Add(AccessLevel.Member);
                return l.ToArray();
            }

            if (this.IsMember)
            {
                l.Add(AccessLevel.Member);
                return l.ToArray();
            }
            return l.ToArray();
        }
    }
}
