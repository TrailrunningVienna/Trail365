using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Trail365.Configuration;
using Trail365.Entities;
using Trail365.UnitTests.TestContext;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    public class TestHostBuilder
    {
        private readonly string TestDirectoryPath;

        public TestHostBuilder(bool useDefaults = true)
        {
            this.TestDirectoryPath = ConfigurationExtension.CreateUniqueTestDirectory();
            if (useDefaults)
            {
                this.Configuration.ConfigureDefaults();
            }
        }

        public static IConfigurationRoot GetConfigurationRoot(Dictionary<string, string> configuration)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(configuration).Build();
        }

        private ITestOutputHelper Helper = null;
        public Dictionary<string, string> Configuration { get; private set; } = new Dictionary<string, string>();

        public TestHostBuilder UseEnvironment(string environment)
        {
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException(nameof(environment));
            this.Configuration["ASPNETCORE_ENVIRONMENT"] = environment;
            return this;
        }

        protected TestHostBuilder UseSqliteWithEmptyTempFolder()
        {
            this.Configuration.ConfigureSqlite();
            return this;
        }

        public TestHost Build()
        {
            return new TestHost(GetConfigurationRoot(this.Configuration), Helper);
        }

        public TestHostBuilder UseCloudStorage(string connectionString, string containerName)
        {
            this.Configuration.ConfigureAzureBlobService(connectionString, containerName);
            return this;
        }

        public TestHostBuilder UseFileSystemStorage(string directoryPath, string requestPath)
        {
            this.Configuration.ConfigureFileSystemBlobService(directoryPath, requestPath);
            return this;
        }

        public TestHostBuilder UseTestOutputHelper(ITestOutputHelper helper)
        {
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
            return this;
        }

        /// <summary>
        /// including IdentityContext because required for lookup!
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static TestHostBuilder DefaultForFrontendAsAdmin(ITestOutputHelper helper = null)
        {
            var result = Empty().UseStaticAuthenticationAsAdmin().WithTrailContext().WithIdentityContext().UseFileSystemStorage();
            if (helper != null)
            {
                result = result.UseTestOutputHelper(helper);
            }
            return result;
        }

        public static TestHostBuilder DefaultForFrontendAsNotLoggedIn(ITestOutputHelper helper = null)
        {
            var result = Empty().UseStaticAuthenticationAsNotLoggedIn().WithTrailContext().WithIdentityContext().UseFileSystemStorage();
            if (helper != null)
            {
                result = result.UseTestOutputHelper(helper);
            }
            return result;
        }

        public static TestHostBuilder DefaultForFrontendAsUser(ITestOutputHelper helper = null)
        {
            var result = Empty().UseStaticAuthenticationAsUser().WithTrailContext().WithIdentityContext().UseFileSystemStorage();
            if (helper != null)
            {
                result = result.UseTestOutputHelper(helper);
            }
            return result;
        }

        /// <summary>
        /// means minimum: authentication yes, dbcontexts NO!
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static TestHostBuilder DefaultForFrontendAsModerator(ITestOutputHelper helper = null)
        {
            var result = Empty().UseStaticAuthenticationAsModerator().WithTrailContext().WithIdentityContext().UseFileSystemStorage();
            if (helper != null)
            {
                result = result.UseTestOutputHelper(helper);
            }
            return result;
        }

        public static TestHostBuilder DefaultForBackend(ITestOutputHelper helper = null)
        {
            var result = Empty().UseStaticAuthenticationAsAdmin().WithTrailContext();
            if (helper != null)
            {
                result = result.UseTestOutputHelper(helper);
            }
            return result;
        }

        public static TestHostBuilder Empty(ITestOutputHelper helper = null)
        {
            var result = new TestHostBuilder().UseSqliteWithEmptyTempFolder().UseEnvironment("Production");

            if (helper != null)
            {
                result = result.UseTestOutputHelper(helper);
            }

            return result;
        }

        public TestHostBuilder WithIdentityContext()
        {
            this.Configuration[nameof(AppSettings.IdentityContextDisabled)] = false.ToString();
            return this;
        }

        public TestHostBuilder WithTaskContext()
        {
            this.Configuration[nameof(AppSettings.TaskContextDisabled)] = false.ToString();
            return this;
        }

        public TestHostBuilder WithTrailContext()
        {
            this.Configuration[nameof(AppSettings.TrailContextDisabled)] = false.ToString();
            return this;
        }

        public string GetBackupDirectoryDefault()
        {
            return Path.Combine(this.TestDirectoryPath, "backup");
        }

        public TestHostBuilder WithBackupDirectory(string backupDirectory = null)
        {
            if (string.IsNullOrEmpty(backupDirectory))
            {
                backupDirectory = this.GetBackupDirectoryDefault();
            }
            this.Configuration[nameof(AppSettings.BackupDirectory)] = backupDirectory;
            return this;
        }

        public TestHostBuilder UseFileSystemStorage(string requestPath = @"/blob")
        {
            string storageFolderPath = Path.Combine(TestDirectoryPath, "blobstorage");
            this.Configuration.ConfigureFileSystemBlobService(storageFolderPath, requestPath);
            return this;
        }

        /// <summary>
        /// NOT working yet... Background is a complicated scenario for testing. Maybe we should NOT do it or doing it in the test-foreground.
        /// </summary>
        /// <returns></returns>
        public TestHostBuilder UseBackgroundTaskEngine()
        {
            this.Configuration[nameof(AppSettings.BackgroundServiceDisabled)] = false.ToString();
            this.WithTaskContext().UseFileSystemStorage(); //gpx track scraping
            return this;
        }

        public TestHostBuilder UseStaticAuthenticationAsAdmin()
        {
            return this.UseStaticAuthentication("admin", nameof(Role.Admin));
        }

        public TestHostBuilder UseStaticAuthenticationAsNotLoggedIn()
        {
            StaticUserSettings settings = new StaticUserSettings
            {
                ShouldNotLogin = true
            };
            this.Configuration.UseStaticUserAuthentication(settings);
            return this;
        }

        public TestHostBuilder UseStaticAuthenticationAsUser()
        {
            return this.UseStaticAuthentication("user", nameof(Role.User));
        }

        public TestHostBuilder UseStaticAuthenticationAsMember()
        {
            return this.UseStaticAuthentication("member", nameof(Role.Member));
        }

        public TestHostBuilder UseStaticAuthenticationAsModerator()
        {
            return this.UseStaticAuthentication("moderator", nameof(Role.Moderator));
        }

        private TestHostBuilder UseStaticAuthentication(string userID, string roles)
        {
            StaticUserSettings settings = new StaticUserSettings
            {
                Roles = roles,
                Name = userID,
                NameIdentifier = userID,
                EMail = userID,
            };

            this.Configuration.UseStaticUserAuthentication(settings);
            return this;
        }
    }
}
