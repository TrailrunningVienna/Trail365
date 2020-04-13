using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Graphics;
using Trail365.Internal;

namespace Trail365.Services
{
    public sealed class ScrapingService
    {
        private readonly BlobService BlobService;

        private readonly ILogger Logger;

        private readonly AppSettings Settings;
        public MapScraper Scraper { get; private set; }

        public ScrapingService(BlobService blobService, ILogger<ScrapingService> logger, MapScraper scraper, IOptionsMonitor<AppSettings> settings)
        {
            this.BlobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Scraper = scraper ?? throw new ArgumentNullException(nameof(scraper));
            this.Settings = settings.CurrentValue ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// this Method does a UPSERT (Insert or Update)
        /// </summary>
        /// <param name="image"></param>
        /// <param name="ms"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Uri UpdateImage(Blob image, Stream ms, string type, string name, IUrlHelper helper)
        {
            //WM 01.09.2019 assuming (means that must be verified) that BlobService.Upload is overwriting the old blob so we don't have to remove the old blob in case of update.
            Guard.AssertNotNull(image);
            Guard.AssertNotNull(ms);
            Guard.Assert(ms.Position == 0);
            Guard.AssertNotNullOrEmptyString(type);
            Guard.AssertNotNullOrEmptyString(name);
            var pngUri = BlobService.Upload(image.ID, type, type, ms, ms.Length, helper);
            ms.Position = 0;
            image.AssignImageSize(ImageAnalyzer.GetSize(ms));
            image.StorageSize = Convert.ToInt32(ms.Length);
            return pngUri;
        }

        private static readonly Size DefaultScrapingSize = new Size(1024, 1024); //DeviceScaleFactor 2!!

        private static readonly Size SmallPreviewSize = new Size(240, 240);

        private static readonly Size MediumPreviewSize = new Size(460, 460); //large 520, XL 800

        /// <summary>
        /// the caller must ensure that the trail object is tracked by the context
        /// changes/creations on images are added to the context by this method!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="trail"></param>
        /// <returns></returns>
        public bool ScrapeTrail(TrailContext context, Trail trail, IUrlHelper helper)
        {
            return this.ScrapeTrail(context, trail, false, helper, CancellationToken.None);
        }

        public static readonly Size ChallengeProfileSize = new Size(400, 50);

        public void BuildElevationProfile(TrailContext context, Trail trail, string gpxXml, IUrlHelper helper, CancellationToken token = default)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var createdImages = new List<Blob>();
            var modifiedImages = new List<Blob>();
            try
            {
                var workItem = trail;

                if (ElevationProfileImage.TryGetElevationProfileData(gpxXml, out var profileData) == false)
                {
                    Logger.LogInformation("ElevationProfile not created because there are no elevation data inside the gpx file");
                    sw.Stop();
                    Logger.LogTrace($"{nameof(BuildElevationProfile)}.Duration={sw.ElapsedMilliseconds} ms");
                }

                token.ThrowIfCancellationRequested();

                this.ImageHandler(context, () => workItem.ElevationProfileImageID, (a) => workItem.ElevationProfileImageID = a, createdImages, modifiedImages, "defaultElevationProfile", helper, ElevationProfileDiagram.Default, profileData, new Size(800, 600));

                this.ImageHandler(context, () => workItem.ElevationProfile_Basic_ImageID, (a) => workItem.ElevationProfile_Basic_ImageID = a, createdImages, modifiedImages, "basicElevationProfile", helper, ElevationProfileDiagram.BasicChallenge, profileData, ChallengeProfileSize);

                this.ImageHandler(context, () => workItem.ElevationProfile_Intermediate_ImageID, (a) => workItem.ElevationProfile_Intermediate_ImageID = a, createdImages, modifiedImages, "intermediateElevationProfile", helper, ElevationProfileDiagram.IntermediateChallenge, profileData, ChallengeProfileSize);

                this.ImageHandler(context, () => workItem.ElevationProfile_Advanced_ImageID, (a) => workItem.ElevationProfile_Advanced_ImageID = a, createdImages, modifiedImages, "advancedElevationProfile", helper, ElevationProfileDiagram.AdvancedChallenge, profileData, ChallengeProfileSize);

                this.ImageHandler(context, () => workItem.ElevationProfile_Proficiency_ImageID, (a) => workItem.ElevationProfile_Proficiency_ImageID = a, createdImages, modifiedImages, "proficiencyElevationProfile", helper, ElevationProfileDiagram.ProficiencyChallenge, profileData, ChallengeProfileSize);

                sw.Stop();
                Logger.LogTrace($"{nameof(BuildElevationProfile)}.Duration={sw.ElapsedMilliseconds}ms");
            }
            finally //Storage sometimes throws exceptions, don't forget to call SaveChanges!
            {
                context.Blobs.AddRange(createdImages);
                context.Blobs.UpdateRange(modifiedImages);
                Logger.LogTrace($"{nameof(BuildElevationProfile)}.DBContext: CreatedImages={createdImages.Count}, ModifiedImage={modifiedImages.Count}");
            }
        }

        public void BuildElevationProfile(TrailContext context, Trail trail, string gpxXml, IUrlHelper helper)
        {
            this.BuildElevationProfile(context, trail, gpxXml, helper, CancellationToken.None);
        }

        private void ImageHandler(TrailContext context, Func<Guid?> getHandler, Action<Guid?> setHandler, List<Blob> createdImages, List<Blob> modifiedImages, string name, IUrlHelper helper, ElevationProfileDiagram diagram, string xml, Size size)
        {
            bool success = ElevationProfileImage.TryCreateImageAsPng(xml, diagram, size, Logger, out var imageData);
            if (!success)
            {
                this.Logger.LogWarning($"{nameof(ImageHandler)}: ElevationProfileImage couln't be calculated for '{name}' (Size={size})");
                return;
            }
            using (var stream = new MemoryStream(imageData))
            {
                this.ImageHandler(context, getHandler, setHandler, createdImages, modifiedImages, stream, name, helper);
            }
        }

        private void ImageHandler(TrailContext context, Func<Guid?> getHandler, Action<Guid?> setHandler, List<Blob> createdImages, List<Blob> modifiedImages, string name, IUrlHelper helper, ElevationProfileDiagram diagram, ElevationProfileData profileData, Size size)
        {
            bool success = ElevationProfileImage.TryCreateImageAsPng(profileData, diagram, size, Logger, out var imageData);
            if (!success)
            {
                this.Logger.LogWarning($"{nameof(ImageHandler)}: ElevationProfileImage couln't be calculated for '{name}' (Size={size})");
                return;
            }
            using (var stream = new MemoryStream(imageData))
            {
                this.ImageHandler(context, getHandler, setHandler, createdImages, modifiedImages, stream, name, helper);
            }
        }

        private void ImageHandler(TrailContext context, Func<Guid?> getHandler, Action<Guid?> setHandler, List<Blob> createdImages, List<Blob> modifiedImages, Stream stream, string name, IUrlHelper helper)
        {
            Guard.Assert(stream.Position == 0);
            var imageID = getHandler();

            if (getHandler().HasValue == false)
            {
                var newImage = new Blob() { FolderName = "png" };
                var pngUri = this.UpdateImage(newImage, stream, "png", name, helper);
                newImage.Url = pngUri.ToString();
                setHandler(newImage.ID);
                createdImages.Add(newImage);
            }
            else
            {
                var oldImage = context.Blobs.Single(i => i.ID == imageID);
                var pngUri = this.UpdateImage(oldImage, stream, "png", name, helper); //UPSERT!
                oldImage.Url = pngUri.ToString();
                modifiedImages.Add(oldImage);
            }
        }

        private void Handler(TrailContext context, Func<Guid?> getHandler, Action<Guid?> setHandler, List<Blob> createdImages, List<Blob> modifiedImages, Stream stream, string name, IUrlHelper helper)
        {
            Guard.Assert(stream.Position == 0);
            var imageID = getHandler();

            if (getHandler().HasValue == false)
            {
                var newImage = new Blob() { FolderName = "png" };
                var pngUri = this.UpdateImage(newImage, stream, "png", name, helper);
                newImage.Url = pngUri.ToString();
                setHandler(newImage.ID);
                createdImages.Add(newImage);
            }
            else
            {
                var oldImage = context.Blobs.Single(i => i.ID == imageID);
                var pngUri = this.UpdateImage(oldImage, stream, "png", name, helper); //UPSERT!
                oldImage.Url = pngUri.ToString();
                modifiedImages.Add(oldImage);
            }
        }

        /// <summary>
        /// the caller must ensure that the trail object is tracked by the context
        /// changes/creations on images are added to the context by this method!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="trail"></param>
        /// <param name="token"></param>
        /// <returns>returns true if there are changes (dirty) on the dbContext</returns>
        public bool ScrapeTrail(TrailContext context, Trail trail, bool debug, IUrlHelper helper, CancellationToken token = default)
        {
            if (trail == null) throw new ArgumentNullException(nameof(trail));
            if (trail.AnalyzerBlob == null) throw new InvalidOperationException($"Trail.GpxBlob must be assigned - please verify EF-Includes");
            this.Logger.LogTrace($"ScrapeTrail: ID={trail.ID.ToString()}, debug={debug.ToString()}");
            Stopwatch sw = Stopwatch.StartNew();
            var createdImages = new List<Blob>();
            var modifiedImages = new List<Blob>();
            try
            {
                var workItem = trail;
                token.ThrowIfCancellationRequested();

                var scraperUrl = new Uri( helper.GetTrailExplorerUrlOrDefault(Settings.TrailExplorerBaseUrl,workItem.AnalyzerBlob.Url ,ExplorerMapStyle.Outdoor, DefaultScrapingSize,true,false));

                //var scraperUrlOld = this.CreatePreviewImageSnapshotUrl(workItem.GpxBlob.Url, DefaultScrapingSize, debug);

                var pngData = this.Scraper.ScreenshotAsync(scraperUrl, DefaultScrapingSize).GetAwaiter().GetResult();

                if (!this.Scraper.IsNull)
                {
                    if (pngData.Length < 1024 * 10)
                    {
                        Logger.LogWarning($"Scraping-Results ignored because to small (TrailID={workItem.ID},GpxUrl {workItem.GpxBlob.Url})");
                        //somethin was wrong with the scraping. do nothing, may be we re-execute on the next round!
                        return false;
                    }
                }

                using (var original = new MemoryStream(pngData))
                {
                    this.Handler(context, () => workItem.PreviewImageID, (a) => workItem.PreviewImageID = a, createdImages, modifiedImages, original, "originalPreview", helper);
                    workItem.ScrapedUtc = DateTime.UtcNow;

                    original.Position = 0;
                    using (var small = this.Scraper.Resize(original, SmallPreviewSize))
                    {
                        this.Handler(context, () => workItem.SmallPreviewImageID, (a) => workItem.SmallPreviewImageID = a, createdImages, modifiedImages, small, "smallPreview", helper);
                    }

                    original.Position = 0;
                    using (var medium = this.Scraper.Resize(original, MediumPreviewSize))
                    {
                        this.Handler(context, () => workItem.MediumPreviewImageID, (a) => workItem.MediumPreviewImageID = a, createdImages, modifiedImages, medium, "mediumPreview", helper);
                    }
                }

                sw.Stop();
                this.Logger.LogTrace($"ScrapeTrail(ID={trail.ID.ToString()}).Duration={sw.ElapsedMilliseconds} msec");
                return true;
            }
            finally //Storage sometimes throws exceptions, don't forget to call SaveChanges!
            {
                context.Blobs.AddRange(createdImages);
                context.Blobs.UpdateRange(modifiedImages);
                this.Logger.LogTrace($"{nameof(ScrapeTrail)}.DBContext: CreatedImages={createdImages.Count}, ModifiedImage={modifiedImages.Count}");
                Debug.WriteLine("oops");
            }
        }

        public Uri CreatePreviewImageSnapshotUrl(string gpxUrl, System.Drawing.Size size, bool debug = false)
        {
            //var blobService = this.Services.GetRequiredService<BlobService>();
            //var gpx = host.BlobService.Upload(trail.ID, "test", "track.gpx", stream, stream.Length);
            ////1. provide gpx over Web
            ////start scrapping using browserless and TrackExplorer
            string sizePart = string.Empty;
            string debugPart = string.Empty;
            string sourcePart = string.Empty;

            if (string.IsNullOrEmpty(gpxUrl) == false)
            {
                string encodedDownloadUri = System.Net.WebUtility.UrlEncode(gpxUrl);
                sourcePart = $"&gpxsource={encodedDownloadUri}";
            }

            if (debug)
            {
                debugPart = $"&debug={debug}";
            }

            if (!size.IsEmpty)
            {
                sizePart = $"&height={size.Height}&width={size.Width}";
            }

            if (string.IsNullOrEmpty(this.Settings.TrailExplorerBaseUrl))
            {
                throw new InvalidOperationException($"AppSettings.{nameof(AppSettings.TrailExplorerBaseUrl)} not defined");
            }

            Uri baseUri = new UriBuilder(this.Settings.TrailExplorerBaseUrl).Uri;

            Uri api = new Uri(baseUri, "/Index");
            UriBuilder builder = new UriBuilder(api.ToString())
            {
                Query = $"mode=snapshot&style=outdoor{sizePart}{debugPart}{sourcePart}"
            };
            return builder.Uri;
        }
    }
}
