using System;
using Trail365.Internal;

namespace Trail365.TileParser
{
    public class TileInfo
    {

        public TileInfo(int x, int y, int zoomLevel)
        {
            this.Column = x;
            this.Row = y;
            this.ZoomLevel = zoomLevel;
            Guard.Assert(this.X == x);
            Guard.Assert(this.Y == y);
        }
        //https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#C.23_2
        /// <summary>
        /// world corrdinates must be converted into tiles coordinates
        /// </summary>
        /// <param name="x">x/longitude/column</param>
        /// <param name="y">y/latitude/row</param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static TileInfo CreateTileInfoFromCoordinate(double x, double y, int zoomLevel)
        {
            if (zoomLevel < 0) throw new IndexOutOfRangeException(nameof(zoomLevel));
            if (zoomLevel > 20) throw new IndexOutOfRangeException(nameof(zoomLevel));

            var pointF = TileMath.WorldToTilePos(x, y, zoomLevel);
            var point = TileMath.Floor(pointF);
            return new TileInfo(point.X, point.Y, zoomLevel);

        }

        /// <summary>
        /// y, latitude, row
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// x, longitude, column
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Column/latitude
        /// </summary>
        public int X => this.Column;

        /// <summary>
        /// Row/longitude
        /// </summary>
        public int Y => this.Row;

        public int ZoomLevel { get; set; }
    }
}
