using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trail365.DTOs;
using Trail365.Entities;

namespace Trail365.Seeds
{

    public class TrailDtoProvider
    {

        public static TrailDtoProvider CreateDummyForPublicSeeds(int count)
        {
            if (count < 1) throw new ArgumentNullException(nameof(count));
            List<TrailDto> list = new List<TrailDto>();

            Random r = new Random();
            for (int i = 1; i < count; i++)
            {
                byte[] gpxContent = null;
                string gpxSourceName = null;
                int gpxSelection = r.Next(0, 2);

                if (gpxSelection == 0)
                {
                    gpxContent = File.ReadAllBytes(GpxTracks.HusarenTempel);
                    gpxSourceName = "Husarentempel";
                };

                if (gpxSelection == 1)
                {
                    gpxContent = File.ReadAllBytes(GpxTracks.Buschberg);
                    gpxSourceName = "Buschberg";
                };

                if (gpxSelection == 2)
                {
                    gpxContent = File.ReadAllBytes(GpxTracks.U4U4Toiflhuette);
                    gpxSourceName = "U4U4Toiflhuette";
                };

                var trail = new TrailDto
                {
                    Name = TextSeedingHelper.GetNameDummy(),
                    Gpx = gpxContent,
                    GpxDownloadAccess = AccessLevel.Public,
                    ListAccess = AccessLevel.Public,
                    Description = $"description for unique test trail {i:000000} using {gpxSourceName}",
                    Excerpt = TextSeedingHelper.GetExcerptDummy(),
                };
                list.Add(trail);
            }
            var p = new TrailDtoProvider
            {
                All = list.ToArray()
            };
            return p;
        }

        public static TrailDtoProvider CreateProductionSeeds(string directory)
        {
            var fileInfos = GpxTracks.GetAllGpxTracksFromDirectory(directory);
            var allDtos = fileInfos.Select(fi => new TrailDto
            {
                Name = Path.GetFileNameWithoutExtension(fi.FullName),
                Gpx = File.ReadAllBytes(fi.FullName),
                GpxDownloadAccess = AccessLevel.Member, //restrictive default
                ListAccess = AccessLevel.Moderator,
            });

            var p = new TrailDtoProvider
            {
                All = allDtos.ToArray()
            };
            return p;
        }

        public static TrailDto CreateUniqueTrail()
        {
            var g = Guid.NewGuid();
            var trail = new TrailDto
            {
                ID = Guid.NewGuid(),
                Name = $"Name for unique test trail {g.ToString("N")}",
                Gpx = File.ReadAllBytes(GpxTracks.HusarenTempel),
                GpxDownloadAccess = AccessLevel.Public,
                ListAccess = AccessLevel.Public,
                Description = $"description for unique test trail {g.ToString("N")}"
            };
            return trail;
        }

        public static TrailDtoProvider CreateInstanceForPublicSeeds()
        {
            var rg = Rosengarten();
            var vtrClassic = VTRClassic();
            var vtrLight = VTRLight();

            vtrLight.Name = "Vienna Trail Run Light";
            vtrLight.ListAccess = AccessLevel.Public;
            vtrLight.GpxDownloadAccess = AccessLevel.Public;

            vtrClassic.Name = "Vienna Trail Run Classic";
            vtrClassic.ListAccess = AccessLevel.Public;
            vtrClassic.GpxDownloadAccess = AccessLevel.Public;

            rg.Name = "Rosengarten Schlern Skymarathon";
            rg.ListAccess = AccessLevel.Public;
            rg.GpxDownloadAccess = AccessLevel.Public;

            var p = new TrailDtoProvider
            {
                All = new TrailDto[]
                {
                    rg,
                    vtrClassic,
                    vtrLight
                }
            };
            return p;
        }

        public static TrailDtoProvider CreateInstance()
        {
            var p = new TrailDtoProvider
            {
                All = new TrailDto[] {
                                                    Rosengarten(),
                                                    VTRClassic(),
                                                    VTRLight(),
                                                    U4U4Toiflhuette(),
                                                    Buschberg(),
                                                    HusarenTempel()
                        }
            };
            return p;
        }

        public static TrailDto Rosengarten() => new TrailDto { ID = new Guid("E38257D6-DBB0-4721-8086-969FD51695AD"), Name = "Public: Rosengarten Schlern", Gpx = File.ReadAllBytes(GpxTracks.Rosengarten), GpxDownloadAccess = AccessLevel.Public, ListAccess = AccessLevel.Public, Description = "Offizieller Veranstaltungstrack 2019" };

        public static TrailDto VTRClassic() => new TrailDto { ID = new Guid("FAACA369-9D42-4A7A-ABD8-F9B0F914EC3F"), Name = "User: Vienna Trail Run Classic", Gpx = File.ReadAllBytes(GpxTracks.VTRClassic), GpxDownloadAccess = AccessLevel.User, ListAccess = AccessLevel.User, Description = "Offizieller Veranstaltungstrack 2019" };

        public static TrailDto VTRLight() => new TrailDto { ID = new Guid("BC9A29AA-1356-430C-99D1-513C9264E480"), Name = "Member: Vienna Trail Run Light", Gpx = File.ReadAllBytes(GpxTracks.VTRLight), GpxDownloadAccess = AccessLevel.Member, ListAccess = AccessLevel.Member, Description = "Offizieller Veranstaltungstrack 2019" };

        public static TrailDto U4U4Toiflhuette() => new TrailDto { ID = new Guid("87C1FA58-E0F6-4D0C-9A18-02962D602B37"), Name = "Admin: U4-U4 Toiflhütte", Gpx = File.ReadAllBytes(GpxTracks.U4U4Toiflhuette), GpxDownloadAccess = AccessLevel.Administrator, ListAccess = AccessLevel.Administrator, Description = "25km/600Hm. Die Höhenmeter sind relativ einfach zu laufen, Wasser bei km 4 und bei km 21." };

        public static TrailDto Buschberg() => new TrailDto { ID = new Guid("61BC4A61-12C2-447B-985C-A2289D4DEA86"), Name = "Moderator: RR-Buschberg", Gpx = File.ReadAllBytes(GpxTracks.U4U4Toiflhuette), GpxDownloadAccess = AccessLevel.Moderator, ListAccess = AccessLevel.Moderator, Description = "Alljährliche 130km Runde am 15. August" };

        public static TrailDto HusarenTempel() => new TrailDto { ID = new Guid("3A303263-F3C8-4C90-80C4-F300BCD21751"), Name = "Moderator: Husarentempel", Gpx = File.ReadAllBytes(GpxTracks.HusarenTempel), GpxDownloadAccess = AccessLevel.Moderator, ListAccess = AccessLevel.Moderator, Description = "Husarentempel und Schubertweg mit Klettersteig" };

        public TrailDto[] All { get; private set; }

        public int PublicTrailsCounter => this.All.Where(t => t.GpxDownloadAccess == AccessLevel.Public).Count();

        public int UserTrailsCounter => this.All.Where(t => t.GpxDownloadAccess == AccessLevel.User).Count();

        public int MemberTrailsCounter => this.All.Where(t => t.GpxDownloadAccess == AccessLevel.Member).Count();

        public int ModeratorTrailsCounter => this.All.Where(t => t.GpxDownloadAccess == AccessLevel.Moderator).Count();

        public int AdministratorTrailsCounter => this.All.Where(t => t.GpxDownloadAccess == AccessLevel.Administrator).Count();
    }
}
