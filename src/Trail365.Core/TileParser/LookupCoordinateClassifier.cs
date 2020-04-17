using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{

    public class LookupCoordinateClassifier : CoordinateClassifier
    {
        private readonly LookupDataProvider LookupDataProvider;

        public override FeatureCollection GetClassification(FeatureCollection input)
        {
            //use LookupDataProvider to get all fact features for envelope
            var facts = this.LookupDataProvider.GetClassifiedMapFeatures(input.GetBoundaries().Envelope);

            //Input MUST be one line (2 points) per Features
            var splitted = input.SplitIntoFeaturePerLineSegment();

            TrackSegement lastSegement = null;

            List<Task<TrackSegement>> tasks = new List<Task<TrackSegement>>();

            for (int i = 0; i < splitted.Count; i++)
            {

                var f = splitted[i];
                LineString currentSegment = (LineString)f.Geometry;

                TrackSegement currentTS = new TrackSegement(this)
                {
                    Line = currentSegment,
                    Previous = lastSegement
                };

                Guard.AssertNotNull(currentSegment);
                Guard.Assert(currentSegment.Count == 2);

                if (lastSegement == null)
                {
                    currentTS.StartCalculation(f, facts);
                    tasks.Add(currentTS.Worker);
                    Guard.Assert(currentTS.Previous == null);
                    lastSegement = currentTS;
                    continue;
                }
                else
                {
                    lastSegement.Next = currentTS;

                    if (lastSegement.HasCalculatedValue)
                    {
                        Guard.Assert(lastSegement.Worker != null);
                        Guard.Assert(currentTS.HasCalculatedValue == false);
                        Guard.Assert(currentTS.Worker == null); //later!
                        currentTS.PrepareInterpolation(f);
                    }
                    else
                    {
                        currentTS.StartCalculation(f, facts);
                        tasks.Add(currentTS.Worker);
                        if (lastSegement.HasCalculatedValue == false && lastSegement.Worker == null)
                        {
                            Guard.Assert(lastSegement.Next == currentTS);
                            lastSegement.StartInterpolation(facts);
                            tasks.Add(lastSegement.Worker);
                        }
                    }
                    lastSegement = currentTS;
                }

            }
            Task.WaitAll(tasks.ToArray());
            return splitted.Merge(includeQuality: true);
        }

        public override CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input)
        {
            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var t = this.GetProposal(facts, input);

            if (t == null)
            {
                return new CoordinateClassification(input, CoordinateClassification.Unknown, "N/A");
            }

            int deviation = t.GetDeviation();

            return new CoordinateClassification(input, t.Classification, deviation.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facts">geoJson file with knowledge about the area</param>
        public LookupCoordinateClassifier(LookupDataProvider lookupDataProvider)
        {
            this.LookupDataProvider = lookupDataProvider ?? throw new ArgumentNullException(nameof(lookupDataProvider));
        }

        private readonly Dictionary<Tuple<int, int>, FeatureCollection> cache = new Dictionary<Tuple<int, int>, FeatureCollection>();


        public ClassificationProposal GetProposal(FeatureCollection facts, Geometry input)
        {
            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (input == null) throw new ArgumentNullException(nameof(input));

            List<Task<Tuple<IFeature, double>>> tasks = new List<Task<Tuple<IFeature, double>>>();

            double terminateDistance = NTSExtensions.Zero;

            foreach (IFeature feature in facts)
            {
                if (feature.Geometry.GeometryType == "Point") continue; //drinking water

                var singleTask = Task.Factory.StartNew<Tuple<IFeature, double>>(() =>
                {
                    return feature.GetDistance(input, terminateDistance);
                });

                tasks.Add(singleTask);
            }

            Task.WaitAll(tasks.ToArray());

            var results = tasks.Where(t => t.Result.Item1 != null).Select(t => t.Result).ToArray(); //move out dist=0.0 for "not found"

            if (results.Length == 0) return null;

            var minValue = results.Select(r => r.Item2).Min();

            var nearest = results.Where(r => r.Item2 == minValue).First();

            string cls = nearest.Item1.Attributes[CoordinateClassifier.OutdoorClassAttributeName].ToString();

            return new ClassificationProposal()
            {
                LookupKey = input,
                Classification = cls,
                DistanceToNearest = nearest.Item2
            };
        }

    }
}
