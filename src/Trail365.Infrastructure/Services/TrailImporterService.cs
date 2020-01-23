using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.Services
{
    public class TrailImporterService
    {
        private readonly BlobService BlobService;

        public TrailImporterService(BlobService blobService)
        {
            BlobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public Func<string, string> TrackPreprocessor { get; set; }

        public TrailImporterTaskResult Execute(TrailContext context, string gpxXml, string externalSource, DateTime utcNow, IUrlHelper helper)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            externalSource = externalSource.Trim().ToLowerInvariant();

            var result = new TrailImporterTaskResult();

            var gpxTracksAsXml = TrailExtender.GpxTrackSplitter(gpxXml).ToList();
            result.Log.AppendLine($"Tracks inside gpx: {gpxTracksAsXml.Count}");
            int counter = -1;
            foreach (var singleXML in gpxTracksAsXml)
            {
                string singlegpx = singleXML;

                if (this.TrackPreprocessor != null)
                {
                    singlegpx = this.TrackPreprocessor(singleXML);
                }

                counter += 1;

                Trail raw = new Trail
                {
                    GpxDownloadAccess = AccessLevel.Moderator,
                    ListAccess = AccessLevel.Moderator,
                };
                Guard.Assert(string.IsNullOrEmpty(raw.Name));
                //read out metadata from file => required as importMetadata!
                TrailExtender.ReadGpxFileInfo(singlegpx, raw);

                result.Log.AppendLine($"Track #{counter}: Name='{raw.Name}', InternalDescription='{raw.InternalDescription}'");

                var existing = context.Trails.Include(t => t.GpxBlob).Where(t => (t.ExternalSource == externalSource) && (t.ExternalID == raw.Name)).FirstOrDefault();

                if (existing != null)
                {
                    //supported changes ?
                    // - Name => NO because sourceID
                    // - InternalDescription => not relevant
                    // - Track changes => YES!

                    bool needsGpxUpdate = false;
                    var current = HashUtils.CalculateHash(singlegpx);
                    if (!string.IsNullOrEmpty(existing.GpxBlob.ContentHash))
                    {
                        if (string.Compare(current, existing.GpxBlob.ContentHash, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            result.Log.AppendLine($"Track #{counter}: NO CHANGES");
                        }
                        else
                        {
                            needsGpxUpdate = true;
                            result.Log.AppendLine($"Track #{counter}: GPX has changed");
                        }
                    }

                    if (needsGpxUpdate)
                    {
                        var existingBlob = existing.GpxBlob;
                        Guard.AssertNotNull(existingBlob);
                        var blobMapping = this.BlobService.UploadXml(singlegpx, existing.GpxBlobID.Value, helper); //gpx stored inside blobStorage => required for scraping!
                        Guard.AssertNotNullOrEmptyString(blobMapping.ContentHash);
                        blobMapping.ApplyToBlob(existingBlob);
                        existingBlob.ModifiedUtc = utcNow;
                        existingBlob.ModifiedByUser = System.Threading.Thread.CurrentPrincipal?.Identity.Name;
                        existingBlob.ContentHash = current;
                        result.UpdatedBlobs.Add(existing.GpxBlob);
                    }
                }
                else
                {
                    //Insert
                    Blob rawBlob = new Blob();
                    raw.GpxBlob = rawBlob;
                    raw.GpxBlobID = rawBlob.ID;
                    var blobMapping = this.BlobService.UploadXml(singlegpx, raw.GpxBlob.ID, helper); //gpx stored inside blobStorage => required for scraping!
                    Guard.AssertNotNullOrEmptyString(blobMapping.ContentHash);
                    blobMapping.ApplyToBlob(rawBlob);
                    raw.ExternalID = raw.Name;
                    Guard.AssertNotNullOrEmptyString(rawBlob.ContentHash);
                    raw.ExternalSource = externalSource;
                    raw.CreatedUtc = utcNow;
                    rawBlob.CreatedUtc = utcNow;
                    result.InsertedTrails.Add(raw);
                    result.InsertedBlobs.Add(rawBlob);
                }
            } //foreach gpx

            context.Trails.AddRange(result.InsertedTrails);
            context.Trails.UpdateRange(result.UpdatedTrails);
            context.Blobs.AddRange(result.InsertedBlobs);
            context.Blobs.UpdateRange(result.UpdatedBlobs);
            result.Changes = context.SaveChanges();
            return result;
        }
    }
}
