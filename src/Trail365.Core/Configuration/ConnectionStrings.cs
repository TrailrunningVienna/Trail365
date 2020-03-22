using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Trail365.Internal;

namespace Trail365.Configuration
{
    public class ConnectionStrings
    {
        public string TrailDB { get; set; }

        public string IdentityDB { get; set; }

        public string TaskDB { get; set; }

        public string CloudStorage { get; set; }

        public string GetResolvedCloudStorageConnectionString()
        {
            var rawConnectionString = string.Format("{0}", this.CloudStorage);
            return System.Environment.ExpandEnvironmentVariables(rawConnectionString).Trim();
        }

        public static bool TryGetSqliteFileInfo(string connectionStringresolved, out System.IO.FileInfo file)
        {
            return TryGetSqliteFileInfo(connectionStringresolved, TextWriter.Null, out file);
        }

        public static bool TryGetSqliteFileInfo(string connectionStringresolved, TextWriter logger, out System.IO.FileInfo file)
        {
            Guard.AssertNotNull(logger);
            logger.WriteLine($"{nameof(TryGetSqliteFileInfo)}: cn='{connectionStringresolved}'");
            file = null;
            if (string.IsNullOrEmpty(connectionStringresolved)) return false;
            var builder = new SqliteConnectionStringBuilder(connectionStringresolved);
            var fileName = builder.DataSource;
            try
            {
                logger.WriteLine($"{nameof(TryGetSqliteFileInfo)}: fileName(raw)='{fileName}'");
                file = new System.IO.FileInfo(fileName);
                logger.WriteLine($"{nameof(TryGetSqliteFileInfo)}: fileName(full)='{file.FullName}', exists={file.Exists}, directory.exists={file.Directory.Exists}");
            }
            catch (Exception ex)
            {
                logger.WriteLine($"{nameof(TryGetSqliteFileInfo)}: HIDDEN EXCEPTION={ex.ToString()}");
                file = null;
            }
            return file != null;
        }

        public string GetResolvedTrailDBConnectionString()
        {
            var rawConnectionString = string.Format("{0}", this.TrailDB);
            return System.Environment.ExpandEnvironmentVariables(rawConnectionString);
        }

        public string GetResolvedIdentityDBConnectionString()
        {
            var rawConnectionString = string.Format("{0}", this.IdentityDB);
            return System.Environment.ExpandEnvironmentVariables(rawConnectionString);
        }

        public string GetResolvedTaskDBConnectionString()
        {
            var rawConnectionString = string.Format("{0}", this.TaskDB);
            return System.Environment.ExpandEnvironmentVariables(rawConnectionString);
        }
    }
}
