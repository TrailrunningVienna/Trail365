using System.Collections.Generic;
using System.Globalization;
using Trail365.TileParser.Contract;

namespace Trail365.TileParser
{
    public static class FeatureParser
    {

        public static VectorTileFeature Parse(TileFeature feature, List<string> keys, List<TileValue> values, uint extent)
        {
            var result = new VectorTileFeature();
            var id = feature.Id;

            var geom = GeometryParser.ParseGeometry(feature.Geometry, feature.Type);
            result.GeometryType = feature.Type;

            // add the geometry
            result.Geometry = geom;
            result.Extent = extent;

            // now add the attributes
            result.Id = id.ToString(CultureInfo.InvariantCulture);
            result.Attributes = AttributesParser.Parse(keys, values, feature.Tags);
            return result;
        }
    }
}
