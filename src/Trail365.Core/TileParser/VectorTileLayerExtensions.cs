using NetTopologySuite.Features;
using System;
namespace Trail365.TileParser
{
    public static class VectorTileLayerExtensions
    {
        public static FeatureCollection Append(this FeatureCollection featureCollection,  VectorTileLayer vectortileLayer, TileInfo tileInfo, Func<Feature,bool> predicate = null  )
        {
            // parameters: tileColumn = 67317, tileRow = 43082, tileLevel = 17 
            //var layerInfos = VectorTileParser.Parse(stream);
            //var fc = layerInfos[0].ToGeoJSON(67317, 43082, 17);


            //var featureCollection = new FeatureCollection();

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

