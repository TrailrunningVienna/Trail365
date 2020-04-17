using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.TileParser;

namespace Trail365
{
    public class VectorTileLookupDataProvider : LookupDataProvider
    {
        public static readonly int DefaultZoomLevel = 13; //test uses 12 to reduce the number of files to provide, 13 may be a good compromiss for smaller items/less loading overhead

        private readonly string TileUri;

        private readonly string BaseName;

        private readonly int ZoomLevel;

        private readonly HttpClient Client;

        public VectorTileLookupDataProvider(string baseUrl) : this(baseUrl, "outdoor", DefaultZoomLevel)
        {

        }

        public VectorTileLookupDataProvider(string baseUrl, string baseName, int zoomLevel)
        {
            this.TileUri = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            this.BaseName = baseName ?? throw new ArgumentNullException(nameof(baseName));
            this.ZoomLevel = zoomLevel;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            this.Client = new HttpClient(handler);
        }

        public static Uri CreateTileUri(string rootUri, string baseName, TileInfo tileInfo)
        {
            string fileUrl = baseName + @$"-{tileInfo.ZoomLevel}/{tileInfo.X}-{tileInfo.Y}.mvt"; //y is row
            var result = new Uri(rootUri).Append(fileUrl);
            return result;
        }


        public static TileInfo[] ConvertoToTileInfos(Geometry envelope, int zoomLevel)
        {
            List<System.Drawing.Point> involvedTilesAsPoint = new List<System.Drawing.Point>();

            foreach (var c in envelope.Coordinates)
            {
                involvedTilesAsPoint.Add(TileMath.Floor(TileMath.WorldToTilePos(c.X, c.Y, zoomLevel)));
            }

            var intermediateList = involvedTilesAsPoint.Distinct().ToList();
            int minX = intermediateList.Select(p => p.X).Min();
            int maxX = intermediateList.Select(p => p.X).Max();
            int minY = intermediateList.Select(p => p.Y).Min();
            int maxY = intermediateList.Select(p => p.Y).Max();

            //add missing tiles to the list (envelope)

            for (int X = minX; X <= maxX; X++)
            {

                for (int Y = minY; Y <= maxY; Y++)
                {
                    intermediateList.Add(new System.Drawing.Point(X, Y));
                }
            }

            var distinctList = intermediateList.Distinct();

            return distinctList.Select(pt => new TileInfo() { Column = pt.X, Row = pt.Y, ZoomLevel = zoomLevel }).ToArray();
        }


        internal async Task<FeatureCollection> InternalGetClassifiedMapFeaturesAsync(Geometry envelope)
        {
            var tileInfos = ConvertoToTileInfos(envelope, this.ZoomLevel);

            FeatureCollection result = new FeatureCollection();

            List<Task<FeatureCollection>> tasks = new List<Task<FeatureCollection>>();

            ContentDownloader downloader = new ContentDownloader(this.Client);

            foreach (var ti in tileInfos)
            {
                Task<FeatureCollection> t = Task.Factory.StartNew<FeatureCollection>(() =>
               {
                   FeatureCollection fc = new FeatureCollection();
                   this.worker(ti, downloader, fc);
                   return fc;
               });
                tasks.Add(t);
            }

            //foreach
            await Task.WhenAll(tasks);

            //collect ALL features from all collections!
            tasks.ForEach(t =>
            {
                foreach (var f in t.Result)
                {
                    result.Add(f);
                }
            });
            return result;
        }


        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, FeatureCollection> Cache = new System.Collections.Concurrent.ConcurrentDictionary<string, FeatureCollection>();

        private void worker(TileInfo ti, ContentDownloader downloader, FeatureCollection result)
        {
            var uri = CreateTileUri(this.TileUri, this.BaseName, ti);
            string key = uri.ToString().ToLowerInvariant();
            if (this.Cache.TryGetValue(key, out var cacheItem))
            {
                foreach (var i in cacheItem)
                {
                    result.Add(i);
                }
                return;
            }

            if (downloader.TryGetContentFromUri(uri, CancellationToken.None, out var task))
            {
                var unCompressedDownloadData = task.GetAwaiter().GetResult();
                using (var stream = new MemoryStream(unCompressedDownloadData))
                {
                    var layerInfos = VectorTileParser.Parse(stream);

                    var li = layerInfos[0];
                    result.Append(li, ti, f =>
                    {
                        return (f.Geometry.GeometryType != "Point");
                    });
                }
            }
            else
            {
                //TODO someone may be empty/missing!
            }
            this.Cache.TryAdd(key, result);
        }

        public override FeatureCollection GetClassifiedMapFeatures(Geometry envelope)
        {
            return this.InternalGetClassifiedMapFeaturesAsync(envelope).GetAwaiter().GetResult();
        }
    }
}
