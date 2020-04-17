using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Trail365.Internal;

namespace Trail365
{
    public static class FeatureCollectionExtensions
    {

        public static int? GetIntValueOrDefault(this Dictionary<string, double> dictionary, string classification)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (string.IsNullOrEmpty(classification)) throw new ArgumentNullException(classification);

            if (dictionary.TryGetValue(classification, out double result))
            {
                return Convert.ToInt32(result);
            }
            else
            {
                return null;
            }
        }

        private static double GetDistanceInMeters(LineString ls)
        {
            double result = 0.0;
            Coordinate last = null;

            foreach (var cs in ls.Coordinates)
            {
                if (last == null)
                {
                    last = cs;
                    continue;
                }

                double currentDist = GeoMath.GetDistanceInMeters(last.X, last.Y, cs.X, cs.Y);
                result += currentDist;
                last = cs;
            }
            return result;
        }

        public static Dictionary<string, double> GetClassifiedDistanceInMeters(this FeatureCollection input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var result = new Dictionary<string, double>();
            foreach (var f in input)
            {
                LineString ls = f.Geometry as LineString;
                if (ls == null)
                {
                    continue; //ignore points from toilets!
                }

                string currentClass = string.Empty;
                if (f.Attributes != null && f.Attributes.Exists(CoordinateClassifier.OutdoorClassAttributeName))
                {
                    currentClass = $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}";
                }
                Guard.Assert(currentClass == currentClass.Trim());
                if (string.IsNullOrEmpty(currentClass)) continue;
                double currentDist = GetDistanceInMeters(ls);
                if (result.TryGetValue(currentClass, out var dist))
                {
                    result[currentClass] = currentDist + dist;
                }
                else
                {
                    result[currentClass] = currentDist;
                }
            } //foreach over input!
            return result;
        }


        /// <summary>
        /// all SingleLine features with the same class are merged into one multiline feature
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FeatureCollection MergeSameClassIntoMultiLine(this FeatureCollection input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            Dictionary<string, List<LineString>> linesPerClass = new Dictionary<string, List<LineString>>();

            foreach (var f in input)
            {
                LineString ls = f.Geometry as LineString;
                if (ls == null)
                {
                    continue; //ignore points from toilets!
                }

                string currentClass = string.Empty;
                if (f.Attributes != null && f.Attributes.Exists(CoordinateClassifier.OutdoorClassAttributeName))
                {
                    currentClass = $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}";
                }

                List<LineString> group;//= new List<LineString>();
                if (!linesPerClass.TryGetValue(currentClass, out group))
                {
                    group = new List<LineString>();
                    linesPerClass.Add(currentClass, group);
                }

                group.Add(ls);
            } //foreach over input!

            var output = new FeatureCollection();

            foreach (string c in linesPerClass.Keys)
            {
                var lol = linesPerClass[c];
                MultiLineString ms = new MultiLineString(lol.ToArray());
                AttributesTable attribs = new AttributesTable();
                if (!string.IsNullOrEmpty(c))
                    attribs.Add(CoordinateClassifier.OutdoorClassAttributeName, c);
                output.Add(new Feature(ms, attribs));
            }
            return output;
        }

        /// <summary>
        /// merges multiple two-point lines into one line, if attibutes are the same
        /// </summary>
        /// <param name="input"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static FeatureCollection Merge(this FeatureCollection input, bool includeQuality)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var output = new FeatureCollection();
            string lastClass = null;
            string lastQuality = null;

            List<LineString> group = new List<LineString>();

            foreach (var f in input)
            {
                LineString ls = f.Geometry as LineString;
                if (ls == null)
                {
                    throw new NotImplementedException("Only Linestrings supported");
                }

                string currentClass = string.Empty;
                string currentQuality = string.Empty;

                if (f.Attributes != null && f.Attributes.Exists(CoordinateClassifier.OutdoorClassAttributeName))
                {
                    currentClass = $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}";
                }

                if  (includeQuality && (f.Attributes != null) && f.Attributes.Exists(CoordinateClassifier.DeviationAttributeName))
                {
                    currentQuality = $"{f.Attributes[CoordinateClassifier.DeviationAttributeName]}";
                }


                if (lastClass == null & lastQuality==null)
                {
                    group.Add(ls);
                    lastClass = currentClass;
                    lastQuality = currentQuality;
                    continue;
                }

                if ( (lastClass == currentClass) && (lastQuality == currentQuality))
                {
                    group.Add(ls);
                }
                else
                {
                    //use group and create a longer string

                    var allPoints = group.SelectMany(g => g.Coordinates).ToArray();

                    LineString line = new LineString(allPoints);

                    var attributes = new AttributesTable();

                    if (!string.IsNullOrEmpty(lastClass))
                    {
                        attributes.Add(CoordinateClassifier.OutdoorClassAttributeName, lastClass);
                    }

                    if (!string.IsNullOrEmpty(lastQuality))
                    {
                        attributes.Add(CoordinateClassifier.DeviationAttributeName, lastQuality);
                    }

                    output.Add(new Feature(line, attributes));
                    group.Clear();
                    group.Add(ls);
                    lastClass = currentClass;
                    lastQuality = currentQuality;
                }
            }

            if (group.Count > 0)
            {
                var allPoints = group.SelectMany(g => g.Coordinates).ToArray();

                LineString line = new LineString(allPoints);

                var attributes = new AttributesTable();

                if (!string.IsNullOrEmpty(lastClass))
                {
                    attributes.Add(CoordinateClassifier.OutdoorClassAttributeName, lastClass);
                }

                if (!string.IsNullOrEmpty(lastQuality))
                {
                    attributes.Add(CoordinateClassifier.DeviationAttributeName, lastQuality);
                }


                output.Add(new Feature(line, attributes));
            }

            return output;
        }

        /// <summary>
        /// returns a high number of Linestrings, each one only with two points (the shortest possible segment lengt) so they can be classified one by one
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FeatureCollection SplitIntoFeaturePerLineSegment(this FeatureCollection input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var output = new FeatureCollection();

            foreach (var f in input)
            {
                LineString ls = f.Geometry as LineString;
                if (ls != null)
                {
                    Coordinate lastPoint = null;
                    foreach (var linePoint in ls.Coordinates)
                    {
                        if (lastPoint == null)
                        {
                            lastPoint = linePoint;
                            continue;
                        }
                        LineString next = new LineString(new Coordinate[] { lastPoint, linePoint });
                        output.Add(new Feature(next, null));
                        lastPoint = linePoint;
                    }
                }
            }
            return output;
        }

        public static void SerializeFeatureCollectionIntoGeoJson(this FeatureCollection featureCollection, string fileName)
        {
            using (var w = File.CreateText(fileName))
            {
                SerializeFeatureCollectionIntoGeoJson(featureCollection, w);
            }
        }

        public static string SerializeFeatureCollectionIntoGeoJson(this FeatureCollection featureCollection)
        {
            var sb = new StringBuilder();
            using (var w = new StringWriter(sb))
            {
                SerializeFeatureCollectionIntoGeoJson(featureCollection, w);
            }
            return sb.ToString();
        }

        public static void SerializeFeatureCollectionIntoGeoJson(this FeatureCollection featureCollection, Stream targetStream)
        {
            using (var writer = new StreamWriter(targetStream, leaveOpen: true))
            {
                SerializeFeatureCollectionIntoGeoJson(featureCollection, writer);
            }
        }

        public static void SerializeFeatureCollectionIntoGeoJson(this FeatureCollection featureCollection, TextWriter writer)
        {
            if (featureCollection == null) throw new ArgumentNullException(nameof(featureCollection));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            JsonSerializer serializer = GeoJsonSerializer.CreateDefault();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Serialize(writer, featureCollection);
            writer.Flush();
        }

        public static Boundaries GetBoundaries(this FeatureCollection featureCollection)
        {

            if (featureCollection == null) throw new ArgumentNullException(nameof(featureCollection));

            if (featureCollection.Count < 1)
            {
                throw new InvalidOperationException("Boundaries cannot be calculated from empty featurecollection");
            }

            var boundaries = featureCollection.Select(feature1 =>
            {
                //tested implementation for single geometry
                var g1 = feature1.Geometry;
                var env1 = g1.Envelope;
                Boundaries result = new Boundaries();
                result.Envelope = env1;
                result.InteriorPoint = env1.InteriorPoint;
                return result;
            }).ToList();

            return boundaries.First();

        }

    }
}
