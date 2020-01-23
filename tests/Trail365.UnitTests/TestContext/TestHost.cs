using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Services;
using Trail365.UnitTests.Utils;
using Trail365.Web;
using Trail365.Web.Controllers;
using Xunit.Abstractions;

namespace Trail365.UnitTests.TestContext
{
    public sealed class TestHost : IDisposable
    {
        public HomeController CreateHomeController(string userEmail)
        {
            var logger = this.ServiceProvider.GetRequiredService<ILogger<HomeController>>();
            var queue = this.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            var serviceScopeFactory = this.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            var cache = this.ServiceProvider.GetRequiredService<IMemoryCache>();
            var homeController = new HomeController(this.TrailContext, this.SettingsMonitor, logger, queue, serviceScopeFactory, cache);
            AttachUser(homeController, userEmail);
            return homeController;
        }

        /// <summary>
        /// sorgt dafür, dass am HttpContext der richtige User anliegt!
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="userEmail"></param>
        public static void AttachUser(Controller controller, string userEmail, string[] roles = null)
        {
            ControllerContext cntx = new ControllerContext();
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEmail)
            };
            if (roles != null)
            {
                foreach (string role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var newClaimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var newPrincipal = new ClaimsPrincipal(newClaimsIdentity);
            cntx.HttpContext = new DefaultHttpContext() { User = newPrincipal };
            controller.ControllerContext = cntx;
        }

        public void WaitForBackgroundTasks(int timeoutSeconds = 10)
        {
            if (timeoutSeconds < 0) throw new ArgumentException("timout must be 0 or greather");
            bool isDisabled = bool.Parse(this.Configuration[nameof(AppSettings.BackgroundServiceDisabled)]);
            if (isDisabled)
            {
                throw new InvalidOperationException("Cannot wait because Background Engine is disabled");
            }
            var hostedService = this.Server.Services.GetRequiredService<IHostedService>();

            var service = hostedService as QueuedHostedService;
            if (service.IsRunning == false)
            {
                throw new InvalidOperationException($"{nameof(QueuedHostedService)} is not running!");
            }

            IBackgroundTaskQueue queue = this.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();

            Stopwatch sw = Stopwatch.StartNew();

            while (!queue.IsEmpty)
            {
                System.Threading.Thread.Sleep((timeoutSeconds + 1) * 10);

                if (sw.Elapsed.TotalSeconds > timeoutSeconds)
                {
                    throw new TimeoutException($"{nameof(this.WaitForBackgroundTasks)} {timeoutSeconds} sec timed out");
                }
            }
        }

        public static void AttachUnauthenticatedUser(Controller controller)
        {
            ControllerContext cntx = new ControllerContext();
            var newClaimsIdentity = new ClaimsIdentity();
            var newPrincipal = new ClaimsPrincipal(newClaimsIdentity);
            cntx.HttpContext = new DefaultHttpContext() { User = newPrincipal };
            controller.ControllerContext = cntx;
        }

        private RequestBuilder CreateRequest(string path)
        {
            return this.EnsureServer().CreateRequest(path);
        }

        public HttpResponseMessage GetFromServer(string path)
        {
            return this.CreateRequest(path).GetAsync().GetAwaiter().GetResult();
        }

        public HttpClient CreateClient()
        {
            return this.EnsureServer().CreateClient();
        }

        public HttpResponseMessage PostToServer(string path, string content)
        {
            var cnt = new StringContent(content, Encoding.UTF8, "application/json");
            var requestBuilder = this.CreateRequest(path)
            .And((req) =>
            {
                req.Headers.Accept.Clear();
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Content = cnt;
            });

            return requestBuilder.PostAsync().GetAwaiter().GetResult();
        }

        public ILogger Logger => NullLogger.Instance;
        public BlobService BlobService => this.ServiceProvider.GetRequiredService<BlobService>();
        public AppSettings Settings => this.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>().CurrentValue;
        public ILoggerFactory LoggerFactory => this.ServiceProvider.GetRequiredService<ILoggerFactory>();
        public IOptionsMonitor<AppSettings> SettingsMonitor => this.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>();
        public IServiceScopeFactory ServiceScopeFactory => this.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        public ScrapingService ScrapingService => this.ServiceProvider.GetRequiredService<ScrapingService>();
        public MapScraper Scraper => this.ServiceProvider.GetRequiredService<MapScraper>();

        public TrailImporterService TrailImporterService => this.ServiceProvider.GetRequiredService<TrailImporterService>();

        public IdentityContext IdentityContext => this.WebHost.Services.GetRequiredService<IdentityContext>();

        public TaskContext TaskContext => this.WebHost.Services.GetRequiredService<TaskContext>();

        public TrailContext TrailContext => this.WebHost.Services.GetRequiredService<TrailContext>();

        private readonly IConfigurationRoot Configuration;
        private readonly ITestOutputHelper OutputHelper;

        public TestHost(IConfigurationRoot configurationRoot, ITestOutputHelper outputHelper = null)
        {
            Configuration = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));
            OutputHelper = outputHelper;
        }

        public IServiceProvider ServiceProvider => this.WebHost.Services;

        public IWebHost WebHost => this.EnsureHost();

        public TestServer Server => this.EnsureServer();

        public IUrlHelper RootUrl
        {
            get
            {
                var server = this.EnsureServer();
                var rootUri = server.BaseAddress;
                return UrlHelperFactory.GetStaticUrlHelper(rootUri);
            }
        }

        private IWebHost EnsureHost() => this.EnsureServer().Host;

        private WebApplicationFactory<Startup> _factory = null;

        private TestServer EnsureServer()
        {
            if (_factory == null)
            {
                _factory = new TestApplicationFactory<Startup>()
                .WithWebHostBuilder(hostBuilder =>
                {
                    hostBuilder.UseConfiguration(this.Configuration);
                    hostBuilder.UseEnvironment(this.Configuration["ASPNETCORE_ENVIRONMENT"]);
                    if (OutputHelper != null)
                    {
                        hostBuilder.ConfigureLogging((builder) =>
                        {
                            builder.AddProvider(new XunitLoggerProvider(OutputHelper))
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .SetMinimumLevel(LogLevel.Trace);
                        });
                    }
                });
            }

            return _factory.Server;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_factory != null)
                    {
                        _factory.Dispose();
                        _factory = null;
                    }
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebServer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
