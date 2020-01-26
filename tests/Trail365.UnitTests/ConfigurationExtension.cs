using System.Collections.Generic;
using System.IO;
using Trail365.Configuration;

namespace Trail365.UnitTests
{
    public static class ConfigurationExtension
    {
        public static Dictionary<string, string> ConfigureDefaults(this Dictionary<string, string> configuration)
        {
            configuration.Add($"{nameof(AppSettings.PuppeteerEnabled)}", false.ToString());
            configuration.Add($"{nameof(AppSettings.AbsoluteExpirationInSecondsRelativeToNow)}", 0.ToString());
            configuration.Add($"{nameof(AppSettings.AllowRobots)}", true.ToString()); //true means: do nothing, don't create a file!
            configuration.Add($"{nameof(AppSettings.IdentityContextDisabled)}", true.ToString()); //true means context is not created, migrations or createdatabase not executed!
            configuration.Add($"{nameof(AppSettings.TrailContextDisabled)}", true.ToString()); //true means context is not created, migrations or createdatabase not executed!
            configuration.Add($"{nameof(AppSettings.TaskContextDisabled)}", true.ToString()); //true means context is not created, migrations or createdatabase not executed!
            configuration.Add($"{nameof(AppSettings.APPINSIGHTS_INSTRUMENTATIONKEY)}", string.Empty);
            configuration.Add($"{nameof(AppSettings.BackgroundServiceDisabled)}", true.ToString());
            return configuration;
        }

        public static Dictionary<string, string> ConfigureAzureBlobService(this Dictionary<string, string> configuration, string connectionString, string containerName)
        {
            configuration.Add($"ConnectionStrings:{nameof(ConnectionStrings.CloudStorage)}", connectionString);
            configuration.Add(nameof(AppSettings.CloudStorageRootContainerName), containerName);
            configuration[nameof(AppSettings.CloudStorageEnabled)] = true.ToString();
            configuration.Add(nameof(AppSettings.FileSystemBlobServiceRootDirectory), string.Empty);
            return configuration;
        }

        public static Dictionary<string, string> ConfigureFileSystemBlobService(this Dictionary<string, string> configuration, string directoryPath, string requestPath)
        {
            configuration[nameof(AppSettings.CloudStorageEnabled)] = false.ToString();
            configuration[nameof(AppSettings.FileSystemBlobServiceRootDirectory)] = directoryPath;
            configuration[nameof(AppSettings.FileSystemBlobServiceRequestPath)] = requestPath;
            return configuration;
        }

        public static string CreateUniqueTestDirectory()
        {
            var uniqueTempFolder = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(uniqueTempFolder);
            return uniqueTempFolder;
        }

        public static Dictionary<string, string> ConfigureSqlite(this Dictionary<string, string> configuration)
        {
            return ConfigureSqlite(configuration, CreateUniqueTestDirectory());
        }

        public static Dictionary<string, string> ConfigureSqlite(this Dictionary<string, string> configuration, string directoryPath)
        {
            var uniqueTempFolder = Path.GetFullPath(directoryPath);

            var resolvedIdentitySqliteFile = Path.Combine(uniqueTempFolder, "identity.test.sqlite");
            configuration.Add($"ConnectionStrings:{nameof(ConnectionStrings.IdentityDB)}", string.Format("Data Source={0}", resolvedIdentitySqliteFile));

            var resolvedTrailSqliteFile = Path.Combine(uniqueTempFolder, "trail365.test.sqlite");
            configuration.Add($"ConnectionStrings:{nameof(ConnectionStrings.TrailDB)}", string.Format("Data Source={0}", resolvedTrailSqliteFile));

            var resolvedTaskSqliteFile = Path.Combine(uniqueTempFolder, "task.test.sqlite");
            configuration.Add($"ConnectionStrings:{nameof(ConnectionStrings.TaskDB)}", string.Format("Data Source={0}", resolvedTaskSqliteFile));

            configuration.Add(nameof(AppSettings.RunMigrationsAtStartup), "true");
            return configuration;
        }

        public static Dictionary<string, string> UseStaticUserAuthentication(this Dictionary<string, string> configuration, StaticUserSettings user)
        {
            configuration.Add(nameof(AppSettings.StaticUserSettingsEnabled), true.ToString());
            configuration.Add(string.Format("{0}:{1}", nameof(AppSettings.StaticUserSettings), nameof(StaticUserSettings.Name)), user.Name);
            configuration.Add(string.Format("{0}:{1}", nameof(AppSettings.StaticUserSettings), nameof(StaticUserSettings.NameIdentifier)), user.NameIdentifier);
            configuration.Add(string.Format("{0}:{1}", nameof(AppSettings.StaticUserSettings), nameof(StaticUserSettings.Roles)), user.Roles);
            configuration.Add(string.Format("{0}:{1}", nameof(AppSettings.StaticUserSettings), nameof(StaticUserSettings.EMail)), user.EMail);
            configuration.Add(string.Format("{0}:{1}", nameof(AppSettings.StaticUserSettings), nameof(StaticUserSettings.ShouldNotLogin)), user.ShouldNotLogin.ToString());
            configuration.Add(string.Format("{0}:{1}", nameof(AppSettings.StaticUserSettings), nameof(StaticUserSettings.UserID)), user.UserID.ToString());
            return configuration;
        }
    }
}
