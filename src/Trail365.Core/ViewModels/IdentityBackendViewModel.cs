using System;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class IdentityBackendViewModel
    {
        public static IdentityBackendViewModel CreateFromIdentity(FederatedIdentity from)
        {
            return new IdentityBackendViewModel
            {
                ID = from.ID,
                Email = from.Email
            };
        }

        public Guid ID { get; set; }
        public string Email { get; set; }
    }
}
