using System;
using System.Linq;
using Trail365.Entities;

namespace Trail365.Seeds
{
    public class TrailEntityProvider
    {
        public static TrailEntityProvider CreateInstance()
        {
            var p = new TrailEntityProvider();
            var rosengarten = new Trail { ID = new Guid("E38257D6-DBB0-4721-8086-969FD51695AD"), Name = "public: Rosengarten Schlern", ListAccess = AccessLevel.Public };
            var vtrClassic = new Trail { ID = new Guid("FAACA369-9D42-4A7A-ABD8-F9B0F914EC3F"), Name = "user: Vienna Trail Run Classic", ListAccess = AccessLevel.User };
            var vtrLight = new Trail { ID = new Guid("BC9A29AA-1356-430C-99D1-513C9264E480"), Name = "member: Vienna Trail Run Light", ListAccess = AccessLevel.Member };
            p.All = new Trail[] { vtrLight, vtrClassic, rosengarten };
            return p;
        }

        public int PublicTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Public).Count();
        public int UserTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.User).Count();

        public int MemberTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Member).Count();

        public int ModeratorTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Moderator).Count();

        public int AdministratorTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Administrator).Count();

        public Trail[] All { get; private set; }
    }
}
