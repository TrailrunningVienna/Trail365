using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetTopologySuite.Features;
using Trail365.TileParser.Contract;

namespace Trail365.TileParser
{
    public static class VectorTileLayerExtensions
    {

        public static IEnumerable<Feature> ReadFeatures(Stream stream, TileInfo ti, Func<Feature, bool> predicate = null)
        {
            var layerInfos = VectorTileParser.Parse(stream);
            var layer = layerInfos.Single(); //developed with single!
            return ReadFeatures(layer, ti, predicate);
        }

        public static IEnumerable<Feature> ReadFeatures(Stream stream, TileInfo ti, bool excludePoints)
        {
            return ReadFeatures(stream, ti,
            f =>
            {
                return (f.Geometry.GeometryType != "Point");
            });
        }

        public static FeatureCollection CreateFrom(string fileName, TileInfo tileInfo, bool excludePoints)
        {
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return CreateFrom(stream, tileInfo, excludePoints);
            }
        }

        public static FeatureCollection CreateFrom(Stream stream, TileInfo tileInfo, bool excludePoints)
        {
            var result = new FeatureCollection();
            foreach (var feature in ReadFeatures(stream, tileInfo, excludePoints))
            {
                result.Add(feature);
            }
            return result;
        }

        public static FeatureCollection Append(this FeatureCollection featureCollection, VectorTileLayer vectortileLayer, TileInfo tileInfo, Func<Feature, bool> predicate = null)
        {
            foreach (var f in ReadFeatures(vectortileLayer, tileInfo, predicate))
            {
                featureCollection.Add(f);
            }
            return featureCollection;
        }

        public static IEnumerable<Feature> ReadFeatures(this VectorTileLayer vectortileLayer, TileInfo tileInfo, Func<Feature, bool> predicate = null)
        {
            _ = vectortileLayer ?? throw new ArgumentNullException(nameof(vectortileLayer));
            foreach (var feature in vectortileLayer.VectorTileFeatures)
            {
                var geojsonFeature = feature.ToGeoJSON(tileInfo);

                if (geojsonFeature.Geometry != null)
                {
                    if (predicate != null)
                    {
                        if (predicate(geojsonFeature))
                        {
                            yield return geojsonFeature;
                        }
                    }
                    else
                    {
                        yield return geojsonFeature;
                    }
                }
            }
        }

    }
}

