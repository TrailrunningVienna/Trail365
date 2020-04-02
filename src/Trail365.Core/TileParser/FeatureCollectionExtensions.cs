using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace Trail365
{
    public static class FeatureCollectionExtensions
    {
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
                if (f.Attributes != null && f.Attributes.Exists("outdoor_class"))
                {
                    currentClass = $"{f.Attributes["outdoor_class"]}";
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
                    attribs.Add("outdoor_class", c);
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
        public static FeatureCollection Merge(this FeatureCollection input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var output = new FeatureCollection();
            string lastClass = null;
            List<LineString> group = new List<LineString>();
            foreach (var f in input)
            {
                LineString ls = f.Geometry as LineString;
                if (ls == null)
                {
                    throw new NotImplementedException("Only Linestrings supported");
                }

                string currentClass = string.Empty;
                if (f.Attributes != null && f.Attributes.Exists("outdoor_class"))
                {
                    currentClass = $"{f.Attributes["outdoor_class"]}";
                }

                if (lastClass == null)
                {
                    group.Add(ls);
                    lastClass = currentClass;
                    continue;
                }

                if (lastClass == currentClass)
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
                        attributes.Add("outdoor_class", lastClass);
                    }
                    output.Add(new Feature(line, attributes));
                    group.Clear();
                    group.Add(ls);
                    lastClass = currentClass;
                }
            }

            if (group.Count > 0)
            {
                var allPoints = group.SelectMany(g => g.Coordinates).ToArray();

                LineString line = new LineString(allPoints);

                var attributes = new AttributesTable();
                if (!string.IsNullOrEmpty(lastClass))
                {
                    attributes.Add("outdoor_class", lastClass);
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
            using (var writer = new StreamWriter(targetStream,leaveOpen:true))
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
