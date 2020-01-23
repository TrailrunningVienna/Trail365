using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    public class User
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string AvatarUrl { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string GivenName { get; set; }

        public Role UserRoles { get; set; } = Role.None;

        public List<FederatedIdentity> Identities { get; set; }
    }
}
