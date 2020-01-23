using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.IO;
using Trail365.Entities;

namespace Trail365
{
    public static class TrailExtender
    {
        public static Trail ReadGpxFileInfo(byte[] gpxContent, Trail target)
        {
            string xml = System.Text.Encoding.UTF8.GetString(gpxContent);
            return ReadGpxFileInfo(xml, target);
        }

        public static GpxFile ReadGpxFileAndBOM(string xml)
        {
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

            if (xml.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                xml = xml.Remove(0, _byteOrderMarkUtf8.Length);
            }
            GpxFile file = GpxFile.Parse(xml, null);
            return file;
        }

        public static IEnumerable<string> GpxTrackSplitter(string xml)
        {
            GpxFile source = ReadGpxFileAndBOM(xml);
            GpxWriterSettings settings = new GpxWriterSettings();

            foreach (var t in source.Tracks)
            {
                GpxFile target = new GpxFile();
                target.Tracks.Add(t);
                target.Metadata = source.Metadata;
                yield return target.BuildString(settings);
            }
        }

        public static Trail ReadGpxFileInfo(string xml, Trail target)
        {
            GpxFile file = ReadGpxFileAndBOM(xml);
            return ReadGpxFileInfo(file, target);
        }

        public static Trail ReadGpxFileInfo(GpxFile file, Trail target)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var gpxTrack = file.Tracks.FirstOrDefault();
            GpxRoute gpxRoute = null;

            ImmutableGpxWaypointTable waypoints = null;
            if (gpxTrack != null)
            {
                var segment = gpxTrack.Segments.FirstOrDefault();
                if (segment != null)
                {
                    waypoints = segment.Waypoints;
                }
            }
            else
            {
                gpxRoute = file.Routes.FirstOrDefault();
                if (gpxRoute != null)
                {
                    waypoints = gpxRoute.Waypoints;
                }
            }

            if (waypoints == null) return target;

            var details = GetDescription(waypoints);

            if (details.Ascent.HasValue)
            {
                target.AscentMeters = System.Convert.ToInt32(details.Ascent);
            }
            if (details.Descent.HasValue)
            {
                target.DescentMeters = System.Convert.ToInt32(details.Descent);
            }

            if (details.Distance.HasValue)
            {
                target.DistanceMeters = System.Convert.ToInt32(details.Distance);
            }

            if (details.AltitudeAtStart.HasValue)
            {
                target.AltitudeAtStart = System.Convert.ToInt32(details.AltitudeAtStart);
            }

            if (details.MaximumAltitude.HasValue)
            {
                target.MaximumAltitude = System.Convert.ToInt32(details.MaximumAltitude);
            }

            if (details.MinimumAltitude.HasValue)
            {
                target.MinimumAltitude = System.Convert.ToInt32(details.MinimumAltitude);
            }
            if (gpxTrack != null)
            {
                if (string.IsNullOrEmpty(gpxTrack.Description) == false)
                {
                    target.InternalDescription = gpxTrack.Description;
                }

                if (string.IsNullOrEmpty(gpxTrack.Name) == false)
                {
                    target.Name = gpxTrack.Name;
                }
                return target;
            }

            if (gpxRoute != null)
            {
                if (string.IsNullOrEmpty(gpxRoute.Description) == false)
                {
                    target.InternalDescription = gpxRoute.Description;
                }

                if (string.IsNullOrEmpty(gpxRoute.Name) == false)
                {
                    target.Name = gpxRoute.Name;
                }
                return target;
            }
            return target;
        }

        public static double GetDistanceInMeters(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public static TrailDescription GetDescription(IEnumerable<GpxWaypoint> sequence)
        {
            GpxWaypoint last = null;
            double? ascent = null;
            double? descent = null;
            double? distance = null;
            double? minAltitude = null;
            double? maxAltitude = null;
            List<double> startAltitudes = new List<double>();
            int counter = 0;
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

                    if (counter < 10)
                    {
                        startAltitudes.Add(current.ElevationInMeters.Value);
                    }
                }
                if (last == null)
                {
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
                distance += GetDistanceInMeters(last.Longitude.Value, last.Latitude.Value, current.Longitude.Value, current.Latitude.Value);
                last = current;
            }

            double? startAltitude = null;
            if (startAltitudes.Count > 0)
            {
                startAltitude = startAltitudes.Average();
            }

            return new TrailDescription { Ascent = ascent, Descent = descent, Distance = distance, MinimumAltitude = minAltitude, MaximumAltitude = maxAltitude, AltitudeAtStart = startAltitude };
        }
    }
}
