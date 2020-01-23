using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.Data
{
    public partial class IdentityContext : DbContext
    {
        internal IQueryable<FederatedIdentity> GetFederatedIdentities(string authenticationType, string nameIdentifier)
        {
            if (string.IsNullOrEmpty(authenticationType)) throw new ArgumentNullException(nameof(authenticationType));
            if (string.IsNullOrEmpty(nameIdentifier)) throw new ArgumentNullException(nameof(nameIdentifier));
            nameIdentifier = nameIdentifier.ToLowerInvariant();
            authenticationType = authenticationType.ToLowerInvariant();
            Guard.Assert(nameIdentifier == nameIdentifier.Trim());
            Guard.Assert(authenticationType == authenticationType.Trim());
            return this.Identities.Where(i => i.Identifier == nameIdentifier && i.AuthenticationType == authenticationType);
        }

        internal IQueryable<FederatedIdentity> GetFederatedIdentities(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (principal.TryGetUserIdClaim(out var userid) == false) throw new InvalidOperationException("Name Identifier Claim not available");
            if (string.IsNullOrEmpty(principal.Identity.AuthenticationType)) throw new ArgumentNullException(nameof(principal.Identity.AuthenticationType));
            return this.GetFederatedIdentities(principal.Identity.AuthenticationType, userid.Value);
        }
    }
}
