using System;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class ProfileSettingsViewModel
    {
        public User ApplyChangesTo(User item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.Name = this.Name;
            item.Surname = this.Surname;
            item.GivenName = this.GivenName;
            item.AvatarUrl = this.AvatarUrl;
            return item;
        }

        public Guid ID { get; set; } = Guid.NewGuid();

        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public string Name { get; set; }

        public int UnreadMessages { get; set; } = 0;

        public string AvatarUrl { get; set; }

        public string Surname { get; set; }

        public string GivenName { get; set; }

        public static ProfileSettingsViewModel CreateFromUser(User user, LoginViewModel login)
        {
            var result = new ProfileSettingsViewModel
            {
                Login = login ?? throw new ArgumentNullException(nameof(login)),
                ID = user.ID,
                Name = user.Name,
                Surname = user.Surname,
                GivenName = user.GivenName,
                AvatarUrl = user.AvatarUrl,
            };
            Internal.Guard.Assert(result.ID == user.ID);
            Internal.Guard.Assert(login.UserID.Value == user.ID);
            return result;
        }
    }
}
