using System;
using NetTopologySuite.Features;
using Trail365.TileParser.Contract;

namespace Trail365.TileParser
{
    public static class VectorTileLayerExtensions
    {
        public static FeatureCollection Append(this FeatureCollection featureCollection, VectorTileLayer vectortileLayer, TileInfo tileInfo, Func<Feature, bool> predicate = null)
        {
            foreach (var feature in vectortileLayer.VectorTileFeatures)
            {
                var geojsonFeature = feature.ToGeoJSON(tileInfo);
                if (geojsonFeature.Geometry != null)
                {
                    if (predicate != null)
                    {
                        if (predicate(geojsonFeature))
                        {
                            featureCollection.Add(geojsonFeature);
                        }
                    }
                    else
                    {
                        featureCollection.Add(geojsonFeature);
                    }
                }
            }
            return featureCollection;
        }
    }
}

