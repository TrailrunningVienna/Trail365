using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class UserBackendViewModel
    {


        private Role _role = Role.None;

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
            item.UserRoles = this._role;
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
                _role = from.UserRoles,
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

        private void Flag(Role flag, bool set)
        {
            // flags |= flag;// SetFlag
            //flags &= ~flag; // ClearFlag
            if (set)
            {
                _role |= flag;
            }
            else
            {
                _role &= ~flag;
            }
        }

        public bool IsModerator
        {
            get
            {
                return _role.HasFlag(Role.Moderator);
            }
            set
            {
                this.Flag(Role.Moderator, value);
            }
        }

        public bool IsMember
        {
            get
            {
                return _role.HasFlag(Role.Member);
            }
            set
            {
                this.Flag(Role.Member, value);
            }
        }

        public bool IsAdministrator
        {
            get
            {
                return _role.HasFlag(Role.Admin);
            }
            set
            {
                this.Flag(Role.Admin, value);
            }
        }

        public bool IsUser
        {
            get
            {
                return _role.HasFlag(Role.User);
            }
            set
            {
                this.Flag(Role.User, value);
            }
        }

    }
}
