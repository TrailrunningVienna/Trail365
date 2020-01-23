using System;

namespace Trail365.Entities
{
    [Flags]
    public enum Role
    {
        None = 0,
        User = 1,
        Member = 2,
        Moderator = 3,
        Admin = 256
    }
}
