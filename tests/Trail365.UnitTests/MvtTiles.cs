using System.IO;
using System.Linq;
using System;
using Trail365.Internal;

namespace Trail365.UnitTests
{
    public static class MvtTiles
    {

        public static Uri GetDirectoryUri()
        {
            DirectoryInfo folder = DirectoryHelper.GetOutputDirectory("MvtTiles", true);
            return new Uri(folder.FullName);
        }

        public static FileInfo[] GetAllMvtTilesFromDirectory(string path)
        {
            return Directory.GetFiles(path, "*.mvt").Select(f => new FileInfo(f)).ToArray();
        }

        private static string EnsureFilePath(string path)
        {
            DirectoryInfo folder = DirectoryHelper.GetOutputDirectory("MvtTiles", true);
            FileInfo file = new FileInfo(Path.Combine(folder.FullName, path));
            if (file.Exists == false)
            {
                throw new FileNotFoundException($"MvtTiles file not found: '{file.FullName}'", file.FullName);
            }
            return file.FullName;
        }

        /// <summary>
        /// file with 157 KBytes
        /// </summary>
        public static string OutdoorZoom2File => EnsureFilePath("outdoor_zoom_2.mbtiles");

        /// <summary>
        /// File with 1.2 MBytes
        /// </summary>
        public static string OutdoorZoom4File => EnsureFilePath("outdoor_zoom_4.mbtiles");
    }
}
