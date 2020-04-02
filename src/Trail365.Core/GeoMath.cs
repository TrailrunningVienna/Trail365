using System;

namespace Trail365
{
    public static class GeoMath
    {
        public static readonly double PIDiv180 = Math.PI / 180.0;
        public static double GetDistanceInMeters(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (PIDiv180);
            var num1 = longitude * (PIDiv180);
            var d2 = otherLatitude * (PIDiv180);
            var num2 = otherLongitude * (PIDiv180) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public static double ToRadians(double angle) //not verified
        {
            return PIDiv180 * angle;
            //return Math.PI * angle / 180;
        }
    }
}
