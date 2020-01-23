using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.Data
{
    public partial class IdentityContext : DbContext, IDependencyTracker
    {
        string IDependencyTracker.OperationTarget => OperationTarget;

        string IDependencyTracker.OperationType(bool cached)
        {
            if (cached)
            {
                return "CACHE-" + OperationType;
            }
            else
            {
                return OperationType;
            }
        }

        public override int SaveChanges()
        {
            using (var tracker = this.DependencyTracker(nameof(SaveChanges)))
            {
                var result = base.SaveChanges();
                tracker.Telemetry.Properties.Add("StateEntriesChanged", result.ToString());
                return result;
            }
        }

        private static readonly string OperationType = "SQLITE";
        private static readonly string OperationTarget = "IdentityContext";

        /// <summary>
        ///
        /// </summary>
        /// <param name="authenticationType"></param>
        /// <param name="nameIdentifier"></param>
        /// <param name="roles"></param>
        /// <returns>false means Identity does not exists</returns>
        public bool TryGetRoles(ClaimsPrincipal principal, out string[] roles, out Guid userID)
        {
            roles = null;
            userID = Guid.Empty;

            using (var tracker = this.DependencyTracker(nameof(GetFederatedIdentities)))
            {
                var resultOrDefault = this.GetFederatedIdentities(principal).Include(fi => fi.User).Select(fi => new { fi.User.UserRoles, fi.UserID }).SingleOrDefault();
                if (resultOrDefault == null)
                {
                    return false;
                }
                roles = resultOrDefault.UserRoles.ToRoleList();
                userID = resultOrDefault.UserID;
                return true;
            }
        }

        public bool TryGetIdentityByNameIdentifier(string authenticationType, string nameIdentifier, out FederatedIdentity identity)
        {
            identity = null;
            using (var tracker = this.DependencyTracker(nameof(GetFederatedIdentities)))
            {
                var identities = this.GetFederatedIdentities(authenticationType, nameIdentifier).ToList();
                if (identities.Count == 0) return false;
                if (identities.Count == 1)
                {
                    identity = identities.First();
                    return true;
                }
            }
            throw new InvalidOperationException("Identity not unique");
        }

        public bool TryGetFederatedIdentity(ClaimsPrincipal principal, out FederatedIdentity identity)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (principal.TryGetUserIdClaim(out var userid) == false) throw new InvalidOperationException("Name Identifier Claim not available");
            if (string.IsNullOrEmpty(principal.Identity.AuthenticationType)) throw new ArgumentNullException(nameof(principal.Identity.AuthenticationType));
            return this.TryGetIdentityByNameIdentifier(principal.Identity.AuthenticationType, userid.Value, out identity);
        }

        public FederatedIdentity CreateIdentity(ClaimsPrincipal principal, bool createRolesFromPrincipal, Guid? proposedUserID)
        {
            var authType = principal.Identity.AuthenticationType.ToLowerInvariant();
            Guard.Assert(authType == authType.Trim());
            Guard.Assert(!string.IsNullOrEmpty(authType));

            if (principal.TryGetUserIdClaim(out var idClaim) == false) throw new InvalidOperationException("User ID Claim not found");
            string nameIdentifier = $"{idClaim.Value}".ToLowerInvariant();
            Guard.Assert(nameIdentifier == nameIdentifier.Trim());
            Guard.Assert(authType == authType.Trim());
            Guard.Assert(!string.IsNullOrEmpty(nameIdentifier));

            if (principal.TryGetMailClaim(out var eClaim) == false) throw new InvalidOperationException("email Claim not found");

            string email = $"{eClaim.Value}".ToLowerInvariant();
            Guard.Assert(email == email.Trim());

            FederatedIdentity identity = new FederatedIdentity
            {
                AuthenticationType = authType,
                Identifier = nameIdentifier,
                Email = email,
            };
            this.Identities.Add(identity);
            Guid? userID = null;
            var userFindings = this.Identities.Where(i => i.Email == email).ToList();

            if (userFindings.Count == 0)
            {
                var rolesDefault = Role.User;
                if (createRolesFromPrincipal)
                {
                    rolesDefault = Role.None;

                    foreach (var cl in principal.Claims)
                    {
                        if (cl.Type == ClaimTypes.Role)
                        {
                            string v = cl.Value;
                            if (System.Enum.TryParse<Role>(v, out var r))
                            {
                                rolesDefault |= r;
                            }
                        }
                    }
                }

                var user = new User
                {
                    UserRoles = rolesDefault
                };

                if (proposedUserID.HasValue)
                {
                    if (proposedUserID.Value == Guid.Empty)
                    {
                        throw new InvalidOperationException("Proposed UserID cannot be Guid.Empty");
                    }

                    user.ID = proposedUserID.Value;
                }
                var gn = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
                if (gn != null)
                {
                    user.GivenName = gn.Value;
                }

                var sn = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                if (sn != null)
                {
                    user.Surname = sn.Value;
                }

                var nm = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (nm != null)
                {
                    user.Name = nm.Value;
                }
                nm = principal.Claims.FirstOrDefault(c => c.Type == "urn:github:name");
                if (nm != null)
                {
                    user.Name = nm.Value;
                }

                this.Users.Add(user);
                userID = user.ID;
            }

            if (userFindings.Count == 1)
            {
                userID = userFindings.Single().ID;
            }

            if (userFindings.Count > 0)
            {
                var distinctList = userFindings.Select(i => i.Email).Distinct().ToList();
                if (distinctList.Count == 1)
                {
                    userID = userFindings.First().ID;
                }
                else
                {
                    throw new InvalidOperationException("we have multiple users for this email!");
                }
            }
            Guard.Assert(userID.HasValue);
            identity.UserID = userID.Value;
            return identity;
        }
    }
}
