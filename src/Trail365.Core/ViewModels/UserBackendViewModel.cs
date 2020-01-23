using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class UserBackendViewModel
    {
        public string GetIdentitiesAsHtml()
        {
            return string.Join("", this.Identities.Select(i => $"<p>{i.Email}</p>"));
        }

        public List<IdentityBackendViewModel> Identities { get; set; } = new List<IdentityBackendViewModel>();

        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public User ApplyChangesTo(User item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.Name = this.Name;
            item.Surname = this.Surname;
            item.GivenName = this.GivenName;
            item.AvatarUrl = this.AvatarUrl;
            return item;
        }

        public static UserBackendViewModel CreateFromUser(User from, bool includeIdentities)
        {
            var vm = new UserBackendViewModel
            {
                ID = from.ID,
                Name = from.Name,
                AvatarUrl = from.AvatarUrl,
                GivenName = from.GivenName,
                Surname = from.Surname,
            };
            if (includeIdentities)
            {
                vm.Identities = from.Identities.Select(id => IdentityBackendViewModel.CreateFromIdentity(id)).ToList();
            }
            return vm;
        }

        public Guid ID { get; set; }

        /// <summary>
        /// username/alias/synonym
        /// </summary>
        public string Name { get; set; }

        public string AvatarUrl { get; set; }

        public string Surname { get; set; }

        public string GivenName { get; set; }
    }
}
