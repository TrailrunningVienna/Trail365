using System;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using Trail365.Internal;

namespace Trail365
{
    public static class NTSExtensions
    {
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
            //terminateDistance stops spending resources if there is a finding inside this distance. if there is no finding it searches again!
            var distOp = new DistanceOp(feature.Geometry, geometry, terminateDistance);
            double distance = distOp.Distance();
            var resultFeature = feature;
            if (distance == Zero)
            {
                resultFeature = null;
                ////perfect match or NO-Finding ? NTS contains '0.0' as some default!
                //var locations = distOp.NearestLocations();
                //if (locations.Length > 1)
                //{
                //    if ((locations[0] == null) && (locations[1] == null))
                //    {
                //        resultFeature = null;
                //    }
                //}
            }
            return new Tuple<IFeature, double>(resultFeature, distance);
        }
    }
}
