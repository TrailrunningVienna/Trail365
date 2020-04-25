using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{

    public class NullCoordinateClassifier : CoordinateClassifier
    {
        public override CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input)
        {
            throw new NotImplementedException();
        }

        public override FeatureCollection GetClassification(FeatureCollection input)
        {
            throw new NotImplementedException();
        }
    }
    public class LookupCoordinateClassifier : CoordinateClassifier
    {
        public bool UseInterpolation { get; set; } = false;

        private readonly LookupDataProvider LookupDataProvider;

        public override FeatureCollection GetClassification(FeatureCollection input)
        {

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

                    if (this.UseInterpolation && lastSegement.HasCalculatedValue)
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

            //return splitted.Merge(includeQuality: true);
            return splitted; //merge useless we have details!
        }
        private static readonly int SoloLimit = 50;
        public override CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input)
        {
            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (input == null) throw new ArgumentNullException(nameof(input));
            string description = null;
            ClassificationProposal proposal = this.GetProposal(facts, input);

            if (proposal == null || proposal.Classifications.Count == 0)
            {
                description = "no proposal";
                return new CoordinateClassification(input, CoordinateClassification.Unknown, "N/A", description);
            }

            int? trailProposal = proposal.GetDeviationOrDefault(CoordinateClassification.Trail);
            int? pavedRoadProposal = proposal.GetDeviationOrDefault(CoordinateClassification.PavedRoad);
            int? asphaltedRoadProposal = proposal.GetDeviationOrDefault(CoordinateClassification.AsphaltedRoad);

            description = $"TrailDeviation={trailProposal}, PavedDeviation={pavedRoadProposal}, AsphaltedDeviation={asphaltedRoadProposal}";

            if ((!trailProposal.HasValue) && (!pavedRoadProposal.HasValue) && (!asphaltedRoadProposal.HasValue))
            {
                throw new InvalidOperationException("oops");
            }

            if (trailProposal.HasValue && trailProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
            if (pavedRoadProposal.HasValue && pavedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
            if (asphaltedRoadProposal.HasValue && asphaltedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);

            if ((asphaltedRoadProposal.HasValue) && (pavedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                int othersMin = Math.Min(asphaltedRoadProposal.Value, pavedRoadProposal.Value);

                if (trailProposal.Value < othersMin) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);

                if (asphaltedRoadProposal.Value < pavedRoadProposal.Value)
                {
                    return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);
                }
                else
                {
                    return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
                }
                throw new InvalidOperationException("decision required");
            }

            if ((pavedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                if (trailProposal.Value < pavedRoadProposal + SoloLimit)
                {
                    return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
                }
                return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
            }


            if ((asphaltedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                if (trailProposal.Value < asphaltedRoadProposal + SoloLimit)
                {
                    return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
                }
                return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);
            }

            //last exit: without limit
            if (trailProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
            if (pavedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
            if (asphaltedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);

            throw new InvalidOperationException("we should never come here");
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

            double terminateDistance = NTSExtensions.DeviationToDistance(10000);

            this.Logger.LogDebug($"{nameof(GetProposal)}: {facts.Count} facts for lookup");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (IFeature feature in facts)
            {
                if (feature.Geometry.GeometryType == "Point") continue; //drinking water

                var singleTask = Task.Factory.StartNew<Tuple<IFeature, double>>(() =>
                {
                    return feature.GetDistance(input, terminateDistance);
                });

                tasks.Add(singleTask);
            }

            this.Logger.LogDebug($"GetDistance Tasks ({tasks.Count}) creation: {sw.Elapsed.ToFormattedDuration()}");
            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            this.Logger.LogDebug($"GetDistance Tasks execution: {sw.Elapsed.ToFormattedDuration()}");

            var notNullResults = tasks.Where(t => t.Result.Item1 != null).Select(t => t.Result).ToList();

            var results = notNullResults.Where(r => r.Item2 < terminateDistance).ToArray(); //move out dist=0.0 for "not found"

            if (results.Length == 0) return null;

            this.Logger.LogTrace($"{results.Length} candidates found");

            var distinctClasses = results.Select(t =>
            {
                return t.Item1.Attributes[CoordinateClassifier.OutdoorClassAttributeName].ToString();
            }).Distinct();

            Dictionary<string, double> classes = new Dictionary<string, double>();

            foreach (string c in distinctClasses)
            {
                var resultsForclass = results.Where(r => r.Item1.Attributes[CoordinateClassifier.OutdoorClassAttributeName].ToString() == c).ToArray();
                if (resultsForclass.Length == 0) continue;
                var minValue = resultsForclass.Select(r => r.Item2).Min();
                var resultsForMin = resultsForclass.Where(r => r.Item2 == minValue).ToArray();
                Tuple<IFeature, double> nearestOne = resultsForMin.First();
                classes.Add(c, nearestOne.Item2);
            }

            return new ClassificationProposal()
            {
                Classifications = classes,
                LookupKey = input,
            };
        }

    }
}
