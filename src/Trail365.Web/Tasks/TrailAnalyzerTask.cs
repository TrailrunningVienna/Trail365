using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
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

        public static FeatureCollection ConvertToFeatureCollection(byte[] buffer, Func<LineString, LineString> simplifierOrDefault = null)
        {
            using (var stream = new MemoryStream(buffer))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return ConvertToFeatureCollection(reader, simplifierOrDefault);
                }
            }
        }

        public static FeatureCollection ConvertToFeatureCollection(string filePath, Func<LineString, LineString> simplifierOrDefault = null)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            using (TextReader reader = File.OpenText(filePath))
            {
                return ConvertToFeatureCollection(reader, simplifierOrDefault);
            }
        }

        public static FeatureCollection ConvertToFeatureCollection(TextReader gpxTextReader, Func<LineString, LineString> simplifierOrDefault)
        {
            Guard.ArgumentNotNull(gpxTextReader, nameof(gpxTextReader));

            using (var reader = XmlReader.Create(gpxTextReader))
            {
                var (metadata, features, extensions) = GpxReader.ReadFeatures(reader, null, GeometryFactory.Default);

                FeatureCollection featureCollection = new FeatureCollection();

                bool multiLineFound = false;
                bool singleLineFound = false;

                foreach (var f in features)
                {
                    MultiLineString ms = f.Geometry as MultiLineString;
                    if (ms != null)
                    {
                        Guard.Assert(multiLineFound == false); //only one feature expected until now!
                        multiLineFound = true;
                        LineString ls = ms.Geometries[0] as LineString;
                        if (ls != null)
                        {
                            Guard.Assert(singleLineFound == false);
                            singleLineFound = true;
                            Feature feature = new Feature();
                            if (simplifierOrDefault != null)
                            {
                                feature.Geometry = simplifierOrDefault(ls);
                            }
                            else
                            {
                                feature.Geometry = ls;
                            }
                            featureCollection.Add(feature);
                        }
                    }
                }
                return featureCollection;
            }
        }


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

            var dbchanges = await scopedDB.SaveChangesAsync();
            //this.Context.DefaultLogger.LogTrace($"{nameof(TrailPreviewTask)}.DBContext.SaveChanges={dbchanges} ({this.Trail.Name})");
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
            FeatureCollection inputData = ConvertToFeatureCollection(buffer); //NOT simplified, we need detailed data here
            FeatureCollection classifiedData = _classifier.GetClassification(inputData);

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
            //logger.LogTrace($"{nameof(this.CalculateTrailAnalysis)}.Start {trail.Name}");
            //logger.LogTrace($"{nameof(this.CalculateTrailAnalysis)}.End {trail.Name}");
        }
    }
}
