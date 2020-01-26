using System.IO;
using Trail365.Internal;

namespace Trail365.Seeds
{
    public static class Markdown
    {
        public static string ChecklistFile => EnsureFilePath("checklist.md");
        private static string EnsureFilePath(string path)
        {
            var folder = DirectoryHelper.GetOutputDirectory("Markdown", true);
            FileInfo file = new FileInfo(Path.Combine(folder.FullName, path));
            if (file.Exists == false)
            {
                throw new FileNotFoundException($"markdown not found: '{file.FullName}'", file.FullName);
            }
            return file.FullName;
        }
    }
}
