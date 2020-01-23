using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PuppeteerSharp;

namespace Trail365.Services
{
    /// <summary>
    /// Core Tasks, avoid any dependency to MVC, Extensions, DI etc....
    /// ILogger may be the only exception!
    /// </summary>
    public class PuppeteerScraper : MapScraper
    {
        private static readonly string OperationType = "PUPPETEER";
        private static readonly string OperationTarget = "browserless.io";

        public TelemetryClient Telemetry
        {
            get
            {
                return _telemetryClient;
            }
        }

        private readonly TelemetryClient _telemetryClient;

        public static PuppeteerScraper Create()
        {
            var config = new TelemetryConfiguration();
            return new PuppeteerScraper(NullLoggerFactory.Instance.CreateLogger<PuppeteerScraper>(), new TelemetryClient(config));
        }

        public PuppeteerScraper(ILogger<PuppeteerScraper> logger, TelemetryClient telemetryClient) : base(logger)
        {
            this._telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        public static async Task<byte[]> CreateScreenshotFromPage(Browser browser, ViewPortOptions vpo, ScreenshotOptions sco, Uri requestUri, ILogger logger)
        {
            var sw = Stopwatch.StartNew();
            var page = await browser.NewPageAsync();
            try
            {
                page.RequestFailed += (sender, args) =>
                {
                    logger.LogError($"Request failed Url={args.Request.Url}");
                };
                page.Error += (sender, args) =>
                {
                    logger.LogError($"Error: {args.Error}");
                };

                page.PageError += (sender, args) =>
                {
                    logger.LogError($"Page Error: {args.Message}");
                };

                await page.SetViewportAsync(vpo);

                //strategy: use more time to get better results!
                await page.GoToAsync(requestUri.ToString(), WaitUntilNavigation.Networkidle0);
                await page.WaitForTimeoutAsync(2500);

                await page.ReloadAsync(waitUntil: new[] { WaitUntilNavigation.Networkidle0 });
                await page.WaitForTimeoutAsync(3500);
                await page.WaitForSelectorAsync("body div");
                await page.WaitForTimeoutAsync(3500);

                logger.LogTrace($"Page loaded: {requestUri}");

                var data = await page.ScreenshotDataAsync(sco);
                sw.Stop();
                logger.LogTrace($"Page screenshoted: Uri={requestUri}, ImageSize={data.Length} Bytes), Duration={sw.ElapsedMilliseconds} msec");
                return data;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public bool FullPage { get; set; } = true;

        /// <summary>
        /// CancellationToken not required because
        /// a) we do onlöy a single scraping
        /// b) Puppeteer Interface has no Cancellation support
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public override async Task<byte[]> ScreenshotAsync(Uri requestUri, System.Drawing.Size size)
        {
            var options = new ConnectOptions()
            {
                BrowserWSEndpoint = $"wss://chrome.browserless.io"//?token={apikey}"
            };

            //https://github.com/GoogleChrome/puppeteer/issues/1329
            var vpo = new ViewPortOptions()
            {
                HasTouch = false,
                IsMobile = false,
                IsLandscape = false,
                DeviceScaleFactor = 2,
                //Height = 1200 * sizeFactor,
                //Width = 630 * sizeFactor
            };

            if (!size.IsEmpty)
            {
                vpo.Height = size.Height;
                vpo.Width = size.Width;
            }

            var sco = new ScreenshotOptions
            {
                Type = ScreenshotType.Png,
                FullPage = this.FullPage,
                BurstMode = false, //https://github.com/GoogleChrome/puppeteer/issues/3502
            };

            using (var operation = this.Telemetry.StartOperation<DependencyTelemetry>(nameof(this.ScreenshotAsync)))
            {
                operation.Telemetry.Target = OperationTarget;
                operation.Telemetry.Type = OperationType;

                using (var browser = await Puppeteer.ConnectAsync(options))
                {
                    return await CreateScreenshotFromPage(browser, vpo, sco, requestUri, Logger);
                } //using browser
            }
        }
    }
}
