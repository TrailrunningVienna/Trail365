using Trail365.Entities;

namespace Trail365
{
    public static class BackendSetup
    {
        public const string Roles = "Admin,Moderator";
        public static readonly Role[] RolesAsEnum = new Role[] { Role.Admin, Role.Moderator };
    }
}
