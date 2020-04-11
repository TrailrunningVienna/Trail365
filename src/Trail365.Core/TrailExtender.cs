using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365
{
    public static class TrailExtender
    {

        public static FeatureCollection ConvertToFeatureCollection(byte[] buffer, Func<LineString, LineString> simplifierOrDefault = null)
        {
            using (var stream = new MemoryStream(buffer))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return ConvertToFeatureCollection(reader, simplifierOrDefault);
                }
            }
        }

        public static FeatureCollection ConvertToFeatureCollection(string filePath, Func<LineString, LineString> simplifierOrDefault = null)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            using (TextReader reader = File.OpenText(filePath))
            {
                return ConvertToFeatureCollection(reader, simplifierOrDefault);
            }
        }

        public static FeatureCollection ConvertToFeatureCollection(TextReader gpxTextReader, Func<LineString, LineString> simplifierOrDefault)
        {
            Guard.ArgumentNotNull(gpxTextReader, nameof(gpxTextReader));

            using (var reader = XmlReader.Create(gpxTextReader))
            {
                var (metadata, features, extensions) = GpxReader.ReadFeatures(reader, null, GeometryFactory.Default);

                FeatureCollection featureCollection = new FeatureCollection();

                bool multiLineFound = false;
                bool singleLineFound = false;

                foreach (var f in features)
                {
                    MultiLineString ms = f.Geometry as MultiLineString;
                    if (ms != null)
                    {
                        Guard.Assert(multiLineFound == false); //only one feature expected until now!
                        multiLineFound = true;
                        LineString ls = ms.Geometries[0] as LineString;
                        if (ls != null)
                        {
                            Guard.Assert(singleLineFound == false);
                            singleLineFound = true;
                            Feature feature = new Feature();
                            if (simplifierOrDefault != null)
                            {
                                feature.Geometry = simplifierOrDefault(ls);
                            }
                            else
                            {
                                feature.Geometry = ls;
                            }
                            featureCollection.Add(feature);
                        }
                    }
                    // route has linestring

                    LineString ls1 = f.Geometry as LineString;
                    if (ls1 != null)
                    {
                        Guard.Assert(multiLineFound == false); //only one feature expected until now!
                        //multiLineFound = true;
                        //LineString ls = ms.Geometries[0] as LineString;
                        if (ls1 != null)
                        {
                            Guard.Assert(singleLineFound == false);
                            singleLineFound = true;
                            Feature feature = new Feature();
                            if (simplifierOrDefault != null)
                            {
                                feature.Geometry = simplifierOrDefault(ls1);
                            }
                            else
                            {
                                feature.Geometry = ls1;
                            }
                            featureCollection.Add(feature);
                        }
                    }


                }
                return featureCollection;
            }
        }


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
                distance += GeoMath.GetDistanceInMeters(last.Longitude.Value, last.Latitude.Value, current.Longitude.Value, current.Latitude.Value);
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
