using System;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using Trail365.Internal;

namespace Trail365
{
    public static class NTSExtensions
    {


        public static double DeviationToDistance(int deviation)
        {
            return (System.Convert.ToDouble(deviation) / DeviationFactor);
        }

        private static readonly int DeviationFactor = 10000000;

        public static int GetDeviation(double distance)
        {
            double derived = distance * DeviationFactor;
            int quality = Convert.ToInt32(Math.Round(derived));
            return quality;
        }

        internal static Tuple<int, int>RoundToArea(this Coordinate pt)
        {
            int x = Convert.ToInt32(Math.Truncate(pt.X));
            int y = Convert.ToInt32(Math.Truncate(pt.Y));
            return new Tuple<int, int>(x, y);
        }

        internal static readonly double Zero = 0.0;

        public static Tuple<IFeature, double> GetDistance(this IFeature feature, Geometry geometry, double terminateDistance)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            Guard.Assert(feature.Geometry.GeometryType.Contains("LineString"));
            var distOp = new DistanceOp(feature.Geometry, geometry, terminateDistance);
            double distance = distOp.Distance();
            var resultFeature = feature;

            if (distance == Zero)
            {
                resultFeature = null;
            }
            if (distance > terminateDistance)
            {
                resultFeature = null;
            }
            return new Tuple<IFeature, double>(resultFeature, distance);
        }
    }
}
