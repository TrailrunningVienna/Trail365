using System;
using System.IO;
using System.Linq;
using Trail365.Entities;
using Trail365.ViewModels;

namespace Trail365.Seeds
{
    public class TrailViewModelProvider
    {
        public static TrailViewModelProvider CreateInstance()
        {
            return new TrailViewModelProvider();
        }

        public static TrailViewModel Rosengarten => new TrailViewModel { ID = new Guid("E38257D6-DBB0-4721-8086-969FD51695AD"), Name = "Public: Rosengarten Schlern", Gpx = File.ReadAllBytes(GpxTracks.Rosengarten), ListAccess = AccessLevel.Public, GpxDownloadAccess = AccessLevel.Public };
        public static TrailViewModel VTRClassic => new TrailViewModel { ID = new Guid("FAACA369-9D42-4A7A-ABD8-F9B0F914EC3F"), Name = "User: Vienna Trail Run Classic", Gpx = File.ReadAllBytes(GpxTracks.VTRClassic), ListAccess = AccessLevel.User, GpxDownloadAccess = AccessLevel.User };
        public static TrailViewModel VTRLight => new TrailViewModel { ID = new Guid("BC9A29AA-1356-430C-99D1-513C9264E480"), Name = "Member: Vienna Trail Run Light", Gpx = File.ReadAllBytes(GpxTracks.VTRLight), ListAccess = AccessLevel.Member, GpxDownloadAccess = AccessLevel.Member };
        public static TrailViewModel U4U4Toiflhuette => new TrailViewModel { ID = new Guid("87C1FA58-E0F6-4D0C-9A18-02962D602B37"), Name = "U4-U4 Toiflhütte", Gpx = File.ReadAllBytes(GpxTracks.U4U4Toiflhuette), ListAccess = AccessLevel.Moderator, GpxDownloadAccess = AccessLevel.Moderator };

        public TrailViewModel[] All => new TrailViewModel[] {
            Rosengarten,
            VTRClassic,
            VTRLight,
            U4U4Toiflhuette,
        };

        public int PublicTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Public).Count();

        public int UserTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.User).Count() + this.PublicTrailsCounter;

        public int MemberTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Member).Count() + this.UserTrailsCounter;

        public int ModeratorTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Moderator).Count() + this.MemberTrailsCounter;

        public int AdministratorTrailsCounter => this.All.Where(t => t.ListAccess == AccessLevel.Administrator).Count() + this.ModeratorTrailsCounter;
    }
}
