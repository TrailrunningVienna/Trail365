using System.IO;
using System.Linq;

namespace Trail365.UnitTests
{
    public static class Helper
    {
        public static string GetTestDataDirectory()
        {
            string data = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (Directory.Exists(data) == false)
            {
                throw new DirectoryNotFoundException(string.Format("Test/Data Folder not available, check csproj!", data));
            }
            return data;
        }

        public static string[] GetTestDataFiles(string searchPattern)
        {
            return Directory.GetFiles(GetTestDataDirectory(), searchPattern, SearchOption.TopDirectoryOnly);
        }

        public static string GetExtendedRultGpxPath()
        {
            return GetTestDataFiles("ExtendedRult-HD.gpx").Single();
        }
    }
}
