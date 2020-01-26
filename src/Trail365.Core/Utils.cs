using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Trail365.Internal;

namespace Trail365
{
    public static class Utils
    {
        private static readonly EnumerationOptions DefaultOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false
        };

        public static bool TryGetLatest(string entryDirectory, string filename, string contextName, TextWriter logger, out FileInfo result)
        {
            if (string.IsNullOrEmpty(entryDirectory)) throw new ArgumentNullException(entryDirectory);
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(filename);
            if (string.IsNullOrWhiteSpace(contextName)) throw new ArgumentNullException(nameof(contextName));

            var part0 = contextName.ToLowerInvariant();
            Guard.Assert(part0 == part0.Trim());


            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.WriteLine($"{nameof(TryGetLatest)}: Start for filename '{filename}' with directory '{entryDirectory}'");
            result = null;

            var lastWriteTime = DateTime.UtcNow.Date;

            for (int i = 0; i < 180; i++)
            {
                var currentDate = lastWriteTime.AddDays(-i);
                var part1 = currentDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

                var startDirectory = Path.Combine(entryDirectory, part0, part1);

                if (!Directory.Exists(startDirectory))
                {
                    logger.WriteLine($"{nameof(TryGetLatest)}: Directory '{startDirectory}' ignored because does not exists");
                    continue;
                }

                var findings = Directory.EnumerateFiles(startDirectory, filename, DefaultOptions).ToList();

                if (findings.Count == 1)
                {
                    result = new FileInfo(findings.Single());
                    logger.WriteLine($"{nameof(TryGetLatest)}: Success (unique): '{result.FullName}'");
                    return true;
                }

                if (findings.Count == 0)
                {
                    logger.WriteLine($"{nameof(TryGetLatest)}: Directory '{startDirectory}' ignored because no file with the name '{filename}' exists");
                    continue;
                }

                logger.WriteLine($"{nameof(TryGetLatest)}: Directory '{startDirectory}' contains {findings.Count} files with name '{filename}' exists");
                var sortedFindings = findings.OrderByDescending(f => f);
                result = new FileInfo(sortedFindings.First());
                logger.WriteLine($"{nameof(TryGetLatest)}: Success (sorted): '{result.FullName}'");
                return true;
            }
            return false;
        }

        public static string GetTargetDirectoryName(string entryDirectory, string contextName, DateTime lastWriteTime)
        {
            if (string.IsNullOrWhiteSpace(entryDirectory)) throw new ArgumentNullException(nameof(entryDirectory));
            if (string.IsNullOrWhiteSpace(contextName)) throw new ArgumentNullException(nameof(contextName));
            var part0 = contextName.ToLowerInvariant();
            Guard.Assert(part0 == part0.Trim());

            var part1 = lastWriteTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture); //"yyyyMMddHHmmssfff"
            var part2 = lastWriteTime.ToString("HHmmss", CultureInfo.InvariantCulture); //"yyyyMMddHHmmssfff"
            var part3 = lastWriteTime.ToString("fff", CultureInfo.InvariantCulture); //"yyyyMMddHHmmssfff"

            var targetDirectory = Path.Combine(entryDirectory, part0, part1, part2, part3);

            return targetDirectory;
        }

        public static string GetTargetFileName(string entryDirectory, string contextName, DateTime lastWriteTime, string fileName)
        {
            //requirements:
            //#1 don't change the filename because easier to copy/move/recover
            //#2 we need a method: "findLatestVersion" of a certain file. It should not be slow!
            //#3 we have different files (Trail, Identity Tasks...)
            //#4 the files have different lastWriteTime
            var targetDirectory = GetTargetDirectoryName(entryDirectory, contextName, lastWriteTime);
            return Path.Combine(targetDirectory, fileName);
        }

        public static byte[] ToBytes(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            Guard.Assert(stream.Position == 0);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        private static readonly char[] InvalidFolderCharacters = new char[] { ' ', '&', '+', '-', '$', '/', (char)92, '[', ']', '(', ')', '{', '}' };

        public static string GetValidFolderName(string proposedValue)
        {
            var value = $"{proposedValue}".ToLowerInvariant().Trim().TrimStart('.').TrimEnd('.');
            if (string.IsNullOrEmpty(value)) throw new InvalidOperationException($"{nameof(proposedValue)} empty (after trim)");
            foreach (Char c in value)
            {
                if (InvalidFolderCharacters.Contains(c))
                {
                    throw new InvalidOperationException($"{nameof(proposedValue)} contains invalid character #{((byte)c).ToString()}");
                }
            }
            return value;
        }
    }
}
