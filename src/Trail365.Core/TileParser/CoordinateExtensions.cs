using System;
using NetTopologySuite.Geometries;

namespace Trail365.TileParser
{
    public static class CoordinateExtensions
    {

        public static Coordinate ToCoordinate(this VectorTileCoordinate coordinate, TileInfo map, uint extent)
        {
            return ToCoordinate(coordinate, map.X, map.Y, map.ZoomLevel, extent);
        }

        public static Coordinate ToCoordinate(this VectorTileCoordinate coordinate, int x, int y, int z, uint extent)
        {
            if (coordinate == null) throw new ArgumentNullException(nameof(coordinate));
            var size = extent * Math.Pow(2, z);
            var x0 = extent * x;
            var y0 = extent * y;

            var y2 = 180 - (coordinate.Y + y0) * 360 / size;
            var lon = (coordinate.X + x0) * 360 / size - 180;
            var lat = 360 / Math.PI * Math.Atan(Math.Exp(y2 * Math.PI / 180)) - 90;
            //WM switch long/lat for mapbox !?
            return new Coordinate(lon,lat);
        }
    }
}
