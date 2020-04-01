using System.Collections.Generic;

namespace Trail365.TileParser
{
    public class ClassifyRings
    {
        // docs for inner/outer rings https://www.mapbox.com/vector-tiles/specification/
        public static List<List<List<VectorTileCoordinate>>> Classify(List<List<VectorTileCoordinate>> rings)
        {
            var polygons = new List<List<List<VectorTileCoordinate>>>();
            List<List<VectorTileCoordinate>> newpoly = null;
            foreach (var ring in rings)
            {
                var poly = new VTPolygon(ring);

                if (poly.IsOuterRing())
                {
                    newpoly = new List<List<VectorTileCoordinate>>() { ring };
                    polygons.Add(newpoly);
                }
                else
                {
                    newpoly?.Add(ring);
                }
            }

            return polygons;
        }
    }
}
