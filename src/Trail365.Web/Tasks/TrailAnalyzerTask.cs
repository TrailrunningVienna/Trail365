using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;
using Trail365.Tasks;

namespace Trail365.Web.Tasks
{
    public class TrailAnalyzerTask : BackgroundTask
    {
        //Logging Strategy: don't use logging here => Framework logging should ensure the minimum!
        public Trail Trail { get; set; }

        protected override void OnBeforeExecute()
        {
            if (this.Trail != null)
            {
                this.Caption = $"{this.Trail.Name}";
            }
            else
            {
                this.Caption = "<Unknown Trail";
            }
        }

        protected override async Task Execute(CancellationToken cancellationToken)
        {
            //load required services/dependencies from DI
            var scopedDB = this.Context.ServiceProvider.GetRequiredService<TrailContext>();
            var classifier = this.Context.ServiceProvider.GetRequiredService<CoordinateClassifier>();
            var blobService = this.Context.ServiceProvider.GetRequiredService<BlobService>();

            var scopedTrail = await scopedDB.Trails.Include(t => t.GpxBlob).Include(t => t.AnalyzerBlob).Where(t => t.ID == this.Trail.ID).SingleOrDefaultAsync();

            await this.CalculateTrailAnalysis(scopedTrail, scopedDB, classifier, this.Context.Url, cancellationToken, this.Context.DefaultLogger, blobService);
            scopedDB.Trails.Update(scopedTrail);
            var dbchanges = await scopedDB.SaveChangesAsync();
            //this.Context.DefaultLogger.LogTrace($"{nameof(TrailPreviewTask)}.DBContext.SaveChanges={dbchanges} ({this.Trail.Name})");
        }


        public static LineString Simplify(LineString lineString)
        {
            NetTopologySuite.Simplify.DouglasPeuckerLineSimplifier simplifier = new NetTopologySuite.Simplify.DouglasPeuckerLineSimplifier(lineString.Coordinates);
            simplifier.DistanceTolerance = 0.00001;
            Coordinate[] smaller = simplifier.Simplify();
            LineString ls = new LineString(smaller);
            return ls;
        }

        public Uri UpdateGeoJsonBlob(BlobService blobService, Blob blob, Stream stream, string type, string name, IUrlHelper helper)
        {
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (blob == null) throw new ArgumentNullException(nameof(blob));
            Guard.AssertNotNull(blob);
            Guard.AssertNotNull(stream);
            Guard.Assert(stream.Position == 0);
            Guard.AssertNotNullOrEmptyString(type);
            Guard.AssertNotNullOrEmptyString(name);
            var pngUri = blobService.Upload(blob.ID, type, type, stream, stream.Length, helper);
            stream.Position = 0;
            blob.StorageSize = Convert.ToInt32(stream.Length);
            return pngUri;
        }

        private async Task CalculateTrailAnalysis(Trail trail, TrailContext _context, CoordinateClassifier _classifier, IUrlHelper urlHelper, CancellationToken cancellationToken, ILogger logger, BlobService blobService)
        {
            if (urlHelper == null) throw new ArgumentNullException(nameof(urlHelper));
            if (trail == null) throw new ArgumentNullException(nameof(trail));
            var contentDownloader = new ContentDownloader();

            var buffer = await contentDownloader.GetFromUriAsync(new Uri(trail.GpxBlob.Url), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            FeatureCollection inputData = TrailExtender.ConvertToFeatureCollection(buffer); //NOT simplified, we need detailed data here

            FeatureCollection classifiedData = _classifier.GetClassification(inputData);

            //use classified to calculate numbers
            var classifications = classifiedData.GetClassifiedDistanceInMeters();
            trail.UnclassifiedMeters = classifications.GetIntValueOrDefault(CoordinateClassification.Unknown);
            trail.UnpavedTrailMeters = classifications.GetIntValueOrDefault(CoordinateClassification.Trail);
            trail.AsphaltedRoadMeters = classifications.GetIntValueOrDefault(CoordinateClassification.AsphaltedRoad);
            trail.PavedRoadMeters = classifications.GetIntValueOrDefault(CoordinateClassification.PavedRoad);

            using (var stream = new MemoryStream())
            {
                classifiedData.SerializeFeatureCollectionIntoGeoJson(stream);
                stream.Position = 0;

                if (!trail.AnalyzerBlobID.HasValue)
                {
                    //Insert
                    var blob = new Blob() { FolderName = "geojson" };
                    var pngUri = this.UpdateGeoJsonBlob(blobService, blob, stream, "geojson", "analyzer", urlHelper);
                    blob.Url = pngUri.ToString();
                    _context.Blobs.Add(blob);
                    trail.AnalyzerBlobID = blob.ID;
                    trail.AnalyzerBlob = blob;
                }
                else
                {
                    //update
                    var oldBlob = _context.Blobs.Single(i => i.ID == trail.AnalyzerBlobID.Value);
                    var pngUri = this.UpdateGeoJsonBlob(blobService, oldBlob, stream, "geojson", "analyzer", urlHelper);
                    oldBlob.Url = pngUri.ToString();
                    _context.Blobs.Update(oldBlob);
                }
            } //memory stream for analyzer geoJson

        }
    }
}
