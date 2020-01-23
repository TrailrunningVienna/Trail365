using System.IO;
using Trail365.Internal;

namespace Trail365.Seeds
{
    public static class Images
    {
        private static string EnsureFilePath(string path)
        {
            var folder = DirectoryHelper.GetOutputDirectory("Images", true);
            FileInfo file = new FileInfo(Path.Combine(folder.FullName, path));
            if (file.Exists == false)
            {
                throw new FileNotFoundException($"Image not found: '{file.FullName}'", file.FullName);
            }
            return file.FullName;
        }

        public static string KahlenbergAsPng => EnsureFilePath("Kahlenberg.png");
        public static string LindkogelAsJpg => EnsureFilePath("Lindkogel.jpg");
        public static string[] All => new string[] { KahlenbergAsPng, LindkogelAsJpg };

        public static string TGHochPath => EnsureFilePath("tg_hoch.jpg");
        public static string TGQuer1Path => EnsureFilePath("tg_quer1.jpg");
        public static string TGQuer2PathAsJpg => EnsureFilePath("tg_quer2.jpg");
        public static string IATF2020AsJpg => EnsureFilePath("iatf2020.jpg");
    }
}
