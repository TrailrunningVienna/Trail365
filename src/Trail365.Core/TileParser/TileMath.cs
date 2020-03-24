using System;
using System.Drawing;

namespace TrackExplorer.Core.TileParser
{



    public static class TileMath
    {
        /// <summary>
        //https://gist.github.com/tmcw/4954720
        //https://github.com/tomchavakis/MBTilesToTMSApi/blob/master/MBTilesToTMS.API/Providers/MBTileProvider.cs
        /// </summary>
        /// <param name="level"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static int Flip(int z, int y)
        {
            int result = (1 << z) - y - 1;
            return result;
        }



        private static double ToRadians(double angle) //not verified
        {
            return Math.PI * angle / 180;
        }

        internal static int long2tilex(double lon, int z)
        {
            return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
        }

        internal static int lat2tiley(double lat, int z)
        {
            return (int)Math.Floor((1 - Math.Log(Math.Tan(ToRadians(lat)) + 1 / Math.Cos(ToRadians(lat))) / Math.PI) / 2 * (1 << z));
        }

        internal static double tilex2long(int x, int z)
        {
            return x / (double)(1 << z) * 360.0 - 180;
        }

        internal static double tiley2lat(int y, int z)
        {
            double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << z);
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }

        public static Point Floor(PointF p)
        {
            int x = (int)Math.Floor(p.X);
            int y = (int)Math.Floor(p.Y);
            return new Point(x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lon">longitude/X</param>
        /// <param name="lat">latitude/Y</param>
        /// <param name="zoom">zoomLevel</param>
        /// <returns></returns>
        internal static PointF WorldToTilePos(double lon, double lat, int zoom)
        {
            PointF p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
            return p;
        }

        ////public TileMap WorldToTilePos(double lon, double lat, int zoom)
        ////{
        ////    PointF p = new Point();
        ////    p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
        ////    p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
        ////        1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

        ////    return p;
        ////}

        //public PointF TileToWorldPos(double tile_x, double tile_y, int zoom)
        //{
        //    PointF p = new Point();
        //    double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

        //    p.X = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
        //    p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

        //    return p;
        //}


    }
}