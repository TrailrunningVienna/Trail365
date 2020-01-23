using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Trail365.Internal
{
    public static class DirectoryHelper
    {
        private static void AppendLog(StringBuilder log, string current)
        {
            log.AppendLine($"CurrentDirectory={current}");
            var subDirs = string.Join(", ", Directory.GetDirectories(current).Select(d => new DirectoryInfo(d)).Select(di => di.Name));
            log.AppendLine($"  SubDirectories: ${subDirs}");
            var parent = Directory.GetParent(current);
            subDirs = string.Join(", ", parent.GetDirectories().Select(di => di.Name));
            log.AppendLine($"ParentDirectory={parent.FullName}");
            log.AppendLine($"  SubDirectories: ${subDirs}");
        }

        /// <summary>
        /// we use Build/Buildoutput to deliver some file/folder
        /// </summary>
        /// <param name="subDirectory"></param>
        /// <returns></returns>
        public static DirectoryInfo GetOutputDirectory(string subDirectory, bool extendedException = false)
        {
            if (string.IsNullOrEmpty(subDirectory)) throw new ArgumentNullException(nameof(subDirectory)); //without the subDir we are not be able to do any verifiaction if we are on the right place!
            var log = new StringBuilder();
            log.AppendLine();
            log.AppendLine($"RequestedSubdirectory: '{subDirectory}'");
            string current = Directory.GetCurrentDirectory();
            if (extendedException)
            {
                AppendLog(log, current);
            }

            DirectoryInfo folder = new DirectoryInfo(Path.Combine(current, subDirectory));
            if (folder.Exists == false)
            {
                var baseUri = new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Uri;
                if (baseUri.Scheme != "file")
                {
                    log.AppendLine($"Unexpected Sheme ({baseUri.Scheme})");
                    throw new InvalidOperationException("Unexpected Sheme");
                }
                var baseFile = new FileInfo(baseUri.AbsolutePath);
                current = baseFile.Directory.FullName;
                AppendLog(log, current);
                folder = new DirectoryInfo(Path.Combine(current, subDirectory));
                if (folder.Exists == false)
                {
                    //log.AppendLine($"Directory={folder.FullName}");
                    throw new DirectoryNotFoundException(log.ToString());
                }
            }
            return folder;
        }
    }
}
