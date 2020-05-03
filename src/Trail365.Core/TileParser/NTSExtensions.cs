using System;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm;
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

        public const int DeviationFactor = 10000000;

        public static int GetDeviation(double distance)
        {
            double derived = distance * DeviationFactor;
            int quality = Convert.ToInt32(Math.Round(derived));
            return quality;
        }

        internal static Tuple<int, int> RoundToArea(this Coordinate pt)
        {
            int x = Convert.ToInt32(Math.Truncate(pt.X));
            int y = Convert.ToInt32(Math.Truncate(pt.Y));
            return new Tuple<int, int>(x, y);
        }

        internal static readonly double Zero = 0.0;

        public static double NormalizedAngle(this Coordinate[] coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            return NormalizedAngle(coordinates[0], coordinates[1]);
        }

        public static double NormalizedAngle(Coordinate p1, Coordinate p2)
        {
            var angle = AngleUtility.Angle(p1, p2);
            angle = AngleUtility.NormalizePositive(angle);
            while (angle >= Math.PI)
            {
                angle = angle - Math.PI;
            }
            return angle;
        }

        public static double NormalizedAngle(LineSegment segment)
        {
            _ = segment ?? throw new ArgumentNullException(nameof(segment));
            return NormalizedAngle(segment.P0, segment.P1);
        }

        public static LineSegmentProposal GetLineSegmentProposalOrDefault(this Geometry mapGeometry, string mapClass, Geometry geometry, double terminateDistance, ILogger logger)
        {
            if (mapGeometry == null) throw new ArgumentNullException(nameof(mapGeometry));
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            if (string.IsNullOrEmpty(mapClass)) throw new ArgumentNullException(nameof(mapClass));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Guard.Assert(geometry.Coordinates.Length == 2);

            var distOp = new DistanceOp(mapGeometry, geometry, terminateDistance);
            double distance = distOp.Distance();
            var resultFeature = mapGeometry;

            if (distance < Zero)
            {
                throw new InvalidOperationException("OOPS");
            }

            if (terminateDistance > Zero)
            {
                if (distance > terminateDistance)
                {
                    //logger.LogTrace($"Out of distance range");
                    return null;
                }
            }

            double geometryAngle = geometry.Coordinates.NormalizedAngle();

            var locations = distOp.NearestLocations();
            Guard.Assert(locations.Length == 2);
            var finding = locations[0];

            var coordinates = finding.GeometryComponent.Coordinates;
            if (coordinates == null || coordinates.Length < 2)
            {
                throw new InvalidOperationException("oops_001");
            }

            var index = Array.IndexOf(coordinates, finding.Coordinate);

            if (index < 0)
            {
                logger.LogWarning($"[{nameof(GetLineSegmentProposalOrDefault)}] Finding not located, to investigate!");
                return null; //To investigate
                //if (coordinates.Length == 2)
                //{
                //    index = 0;
                //}
                //else
                //{
                //    throw new InvalidOperationException("oops_002");
                //}
            }

            LineSegment segment1 = null;
            LineSegment segment2 = null;

            double? angle1 = null;
            double? angle2 = null;
            double? diff1 = null;
            double? diff2 = null;

            if (index < coordinates.Length - 1)
            {
                segment1 = new LineSegment(coordinates[index], coordinates[index + 1]);
                angle1 = NTSExtensions.NormalizedAngle(segment1);
                diff1 = AngleUtility.Diff(geometryAngle, angle1.Value);
            }

            if (index > 0)
            {
                segment2 = new LineSegment(coordinates[index], coordinates[index - 1]);
                angle2 = NTSExtensions.NormalizedAngle(segment2);
                diff2 = AngleUtility.Diff(geometryAngle, angle2.Value);
            }

            double? selectedAngle = null;
            LineSegment selectedSegment = null;

            if (angle1.HasValue && angle2.HasValue)
            {
                if (diff1.Value < diff2.Value)
                {
                    selectedAngle = angle1;
                    selectedSegment = segment1;
                }
                else
                {
                    selectedAngle = angle2;
                    selectedSegment = segment2;
                }
            }
            else
            {
                if (angle1.HasValue)
                {
                    selectedAngle = angle1;
                    selectedSegment = segment1;
                }
                else
                {
                    selectedAngle = angle2;
                    selectedSegment = segment2;
                }
            }

            LineSegmentProposal llsOnGeometry = new LineSegmentProposal(geometry)
            {
                Segement = null,
                NormalizedAngle = NTSExtensions.NormalizedAngle(geometry.Coordinates)
            };

            return new LineSegmentProposal(mapGeometry)
            {
                Segement = selectedSegment,
                NormalizedAngle = selectedAngle.Value,
                Reference = llsOnGeometry,
                ReferenceDistance = distance,
                Classification = mapClass
            };
        }

        public static Tuple<IFeature, double, double> GetDistance(this IFeature feature, Geometry geometry, double terminateDistance)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            Guard.Assert(feature.Geometry.GeometryType.Contains("LineString"));
            Guard.Assert(geometry.Coordinates.Length == 2);

            var distOp = new DistanceOp(feature.Geometry, geometry, terminateDistance);
            double distance = distOp.Distance();
            var resultFeature = feature;

            if (distance < Zero)
            {
                throw new InvalidOperationException("OOPS");
            }

            if (terminateDistance > Zero)
            {
                if (distance > terminateDistance)
                {
                    resultFeature = null;
                }
            }

            double geometryAngle = geometry.Coordinates.NormalizedAngle();

            var locations = distOp.NearestLocations();
            Guard.Assert(locations.Length == 2);
            var finding = locations[0];

            var coordinates = finding.GeometryComponent.Coordinates;
            if (coordinates == null || coordinates.Length < 2) throw new InvalidOperationException("oops_001");
            var index = Array.IndexOf(coordinates, finding.Coordinate);
            if (index < 0) throw new InvalidOperationException("oops_002");

            double? angle1 = null;
            double? angle2 = null;
            double? diff1 = null;
            double? diff2 = null;

            if (index < coordinates.Length - 1)
            {
                angle1 = NTSExtensions.NormalizedAngle(coordinates[index], coordinates[index + 1]);
                diff1 = AngleUtility.Diff(geometryAngle, angle1.Value);
            }

            if (index > 0)
            {
                angle2 = NTSExtensions.NormalizedAngle(coordinates[index], coordinates[index - 1]);
                diff2 = AngleUtility.Diff(geometryAngle, angle2.Value);
            }

            double? selectedAngle = null;

            if (angle1.HasValue && angle2.HasValue)
            {
                if (diff1.Value < diff2.Value)
                {
                    selectedAngle = angle1;
                }
                else
                {
                    selectedAngle = angle2;
                }
            }
            else
            {
                selectedAngle = (angle1.HasValue) ? angle1.Value : angle2.Value;
            }

            return new Tuple<IFeature, double, double>(resultFeature, distance, selectedAngle.Value);
        }

    }
}
