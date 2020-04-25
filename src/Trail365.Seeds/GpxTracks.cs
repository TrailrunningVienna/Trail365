using System.IO;
using System.Linq;
using Trail365.Internal;

namespace Trail365.Seeds
{
    public static class GpxTracks
    {
        public static FileInfo[] GetAllGpxTracksFromDirectory(string path)
        {
            return Directory.GetFiles(path, "*.gpx").Select(f => new FileInfo(f)).ToArray();
        }

        public static string[] GpxFilesInFolder => DirectoryHelper.GetOutputDirectory("GpxTracks", true).GetFiles("*.gpx", SearchOption.TopDirectoryOnly).Select(fi => fi.FullName).ToArray();

        private static string EnsureFilePath(string path)
        {
            DirectoryInfo folder = DirectoryHelper.GetOutputDirectory("GpxTracks", true);
            FileInfo file = new FileInfo(Path.Combine(folder.FullName, path));
            if (file.Exists == false)
            {
                throw new FileNotFoundException($"GpxTrack not found: '{file.FullName}'", file.FullName);
            }
            return file.FullName;
        }

        public static string MultiFile3Sample => EnsureFilePath("MultiFile3Sample.gpx");
        public static string Rosengarten => EnsureFilePath("RosengartenSchlernSkymarathon2019.gpx");
        public static string VTRClassic => EnsureFilePath("VTR Classic 2019.gpx");
        public static string VTRLight => EnsureFilePath("VTR Light 2019.gpx");
        public static string U4U4Toiflhuette => EnsureFilePath("U4-U4Toiflhuette.gpx");
        public static string Buschberg => EnsureFilePath("Buschberg.gpx");
        public static string HusarenTempel => EnsureFilePath("HusarenTempel-Schubertweg.gpx");

        public static string Wanderweg41 => EnsureFilePath("41er.1.1.0.gpx");

        /// <summary>
        /// caused some exception that should be solved/regression tested
        /// it is a route not a track!
        /// </summary>
        public static string SharedRoute => EnsureFilePath("shared-route.gpx"); 

        /// <summary>
        /// Ignore Multifile
        /// </summary>
        public static string[] AllValidTracks => new string[] { Rosengarten, VTRClassic, VTRLight, U4U4Toiflhuette, Buschberg, HusarenTempel };
    }
}
