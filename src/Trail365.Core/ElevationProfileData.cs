using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NetTopologySuite.IO;

namespace Trail365
{
    public class ElevationProfileData
    {
        public ElevationProfileData(float distance, float minAltitude, float maxAltitude, IEnumerable<PointF> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            this.Distance = distance;
            this.MinAltitude = minAltitude;
            this.MaxAltitude = maxAltitude;
            this.Line = points.ToArray();
        }

        public ElevationProfileData(double distance, double minAltitude, double maxAltitude, IEnumerable<PointF> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            this.Distance = Convert.ToSingle(distance);
            this.MinAltitude = Convert.ToSingle(minAltitude);
            this.MaxAltitude = Convert.ToSingle(maxAltitude);
            this.Line = points.ToArray();
        }

        public PointF[] Line { get; private set; }

        public float Distance { get; private set; }

        public float MinAltitude { get; private set; }

        public float MaxAltitude { get; private set; }

        public static bool TryGetElevationProfileData(IEnumerable<GpxWaypoint> sequence, out ElevationProfileData data)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            GpxWaypoint last = null;
            double? ascent = null;
            double? descent = null;
            double? distance = null;
            double? minAltitude = null;
            double? maxAltitude = null;
            int counter = 0;

            List<PointF> line = new List<PointF>();

            foreach (var current in sequence)
            {
                counter += 1;
                if (current.ElevationInMeters.HasValue)
                {
                    if (!minAltitude.HasValue)
                    {
                        minAltitude = current.ElevationInMeters.Value;
                    }
                    else
                    {
                        minAltitude = Math.Min(current.ElevationInMeters.Value, minAltitude.Value);
                    }

                    if (!maxAltitude.HasValue)
                    {
                        maxAltitude = current.ElevationInMeters.Value;
                    }
                    else
                    {
                        maxAltitude = Math.Max(current.ElevationInMeters.Value, maxAltitude.Value);
                    }
                }
                if (last == null)
                {
                    if (current.ElevationInMeters.HasValue)
                    {
                        PointF p = new PointF(0, Convert.ToSingle(current.ElevationInMeters.Value));
                        line.Add(p);
                    }

                    last = current;
                    continue;
                }
                if ((current.ElevationInMeters.HasValue) && (last.ElevationInMeters.HasValue))
                {
                    var deltaAltitude = current.ElevationInMeters.Value - last.ElevationInMeters.Value;
                    if (deltaAltitude > 0d)
                    {
                        if (ascent.HasValue == false)
                        {
                            ascent = 0d;
                        }
                        ascent += deltaAltitude;
                    }
                    else
                    {
                        if (descent.HasValue == false)
                        {
                            descent = 0d;
                        }
                        descent += -deltaAltitude;
                    }
                }
                if (distance.HasValue == false)
                {
                    distance = 0d;
                }
                distance += GeoMath.GetDistanceInMeters(last.Longitude.Value, last.Latitude.Value, current.Longitude.Value, current.Latitude.Value);
                if (current.ElevationInMeters.HasValue)
                {
                    PointF p = new PointF(Convert.ToSingle(distance.Value), Convert.ToSingle(current.ElevationInMeters.Value));
                    line.Add(p);
                }
                last = current;
            }

            if ((!minAltitude.HasValue) || (!maxAltitude.HasValue))
            {
                data = null;
                return false;
            }

            data = new ElevationProfileData(distance.Value, minAltitude.Value, maxAltitude.Value, line.ToArray());
            return true;
        }
    }
}
