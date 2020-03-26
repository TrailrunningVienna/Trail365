using System.Drawing;
using System;

namespace Trail365.TileParser
{
    public class TileInfo
    {

        //https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#C.23_2



        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">x/longitude/column</param>
        /// <param name="y">y/latitude/row</param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static TileInfo Create(double x, double y, int zoomLevel)
        {
            if (zoomLevel < 0) throw new IndexOutOfRangeException(nameof(zoomLevel));
            if (zoomLevel > 20) throw new IndexOutOfRangeException(nameof(zoomLevel));

            var pointF = TileMath.WorldToTilePos(x, y, zoomLevel);
            var point = TileMath.Floor(pointF);

            return new TileInfo()
            {
                Row = point.X,
                Column = point.Y,
                ZoomLevel = zoomLevel
            };
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
