using System.IO;
using Trail365.Internal;

namespace Trail365.Seeds
{
    public static class FBEvents
    {
        public static string FB01112019 => EnsureFilePath("FB01112019.json");
        public static string FBFurth => EnsureFilePath("FBFurth.json");

        private static string EnsureFilePath(string path)
        {
            var folder = DirectoryHelper.GetOutputDirectory("FBEvents", true);
            FileInfo file = new FileInfo(Path.Combine(folder.FullName, path));
            if (file.Exists == false)
            {
                throw new FileNotFoundException($"Json not found: '{file.FullName}'", file.FullName);
            }
            return file.FullName;
        }
    }
}
