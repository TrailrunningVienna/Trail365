using System.IO;

namespace Trail365.Configuration
{
    public class AppSettings
    {
        public bool TryGetResolvedBackupDirectory(out DirectoryInfo directory)
        {
            directory = null;
            if (string.IsNullOrEmpty(this.BackupDirectory)) return false;
            var resolved = System.Environment.ExpandEnvironmentVariables(this.BackupDirectory);
            var absolute = Path.GetFullPath(resolved);
            directory = new DirectoryInfo(absolute);
            return true;
        }

        public bool EnableForwardHeaders { get; set; } = false;

        public string BackupDirectory { get; set; }

        public bool BackgroundServiceDisabled { get; set; }

        public bool TrailContextDisabled { get; set; } = false;

        public bool IdentityContextDisabled { get; set; } = false;

        public bool TaskContextDisabled { get; set; } = false;

        public int ResponseCacheDurationSeconds { get; set; } = 10;

        public bool StaticUserSettingsEnabled { get; set; }

        /// <summary>
        /// Attention: if enabled, the default auth is automatically disabled (not loaded into middlewatre)
        /// </summary>
        public StaticUserSettings StaticUserSettings { get; set; }

        public bool CloudStorageEnabled { get; set; } = false;

        public bool PuppeteerEnabled { get; set; } = true;

        public string CloudStorageRootContainerName { get; set; }

        public string FileSystemBlobServiceRootDirectory { get; set; }

        public string GetFileSystemBlobServiceRootDirectoryResolved()
        {
            var raw = string.Format("{0}", this.FileSystemBlobServiceRootDirectory);
            return System.Environment.ExpandEnvironmentVariables(raw);
        }

        public string ApplicationInsightsStorageFolder { get; set; }

        public bool FileSystemBlobServiceBrowserEnabled { get; set; }

        public string FileSystemBlobServiceRequestPath { get; set; }

        /// <summary>
        /// production default 60*60*24*7 => 7 days
        /// </summary>
        public int CloudStorageMaxAgeSeconds { get; set; } = 60 * 60 * 24 * 7;  //TODO rename, it is also used for FileSystemBlob!

        public bool HasInstrumentationKey()
        {
            return !string.IsNullOrEmpty(this.APPINSIGHTS_INSTRUMENTATIONKEY);
        }

        public string APPINSIGHTS_INSTRUMENTATIONKEY { get; set; }

        public bool WEBSITES_ENABLE_APP_SERVICE_STORAGE { get; set; } = true;

        /// <summary>
        /// used for Paging
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// used like a pagesize in cases where no paging is implemented (example: news-stream)
        /// </summary>
        public int MaxResultSize { get; set; } = 50;

        /// <summary>
        /// features like news-stream can "promote" some item. This is the maximum number of items that can be promoted.
        /// </summary>
        public int MaxPromotionSize { get; set; } = 7;

        /// <summary>
        /// Liste von email Adressen welche "automatisch" Admin sind
        /// reine eMailAdressen, getrennt durch Komma, Strichpunkt oder Pipe
        /// </summary>
        public string AdminUsers { get; set; }

        public bool RunMigrationsAtStartup { get; set; } = false;
        public bool SeedOnCreation { get; set; }

        /// <summary>
        /// required for seeding using the FileSystemBlobService
        /// </summary>
        public string SeedingApplicationUrl { get; set; }

        public bool FacebookAuthentication { get; set; }

        public bool DisableImageDelivery { get; set; } = false;

        public bool GoogleAuthentication { get; set; }

        public bool GitHubAuthentication { get; set; }

        public GoogleSettings GoogleSettings { get; set; }

        public FacebookSettings FacebookSettings { get; set; } = new FacebookSettings();

        public GitHubSettings GitHubSettings { get; set; }

        public bool SyncEnabled { get; set; } = false;
        public bool SyncOverwriteEnabled { get; set; } = false;

        /// <summary>
        /// if false, then we provide a robots.txt with "denyAll"
        /// if true then we don't deliver any robots.txt
        /// </summary>
        public bool AllowRobots { get; set; } = false;

        public Features Features { get; set; } = new Features();

        public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    }
}
