using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{

    /// <summary>
    /// LineString (multiple points, more then two, logical piece of work
    /// Gpx-Track: split into a few "threads"
    /// </summary>
    public class TrackSegement
    {
        private readonly FeatureCollection Facts;
        public double TerminateDistance { get; private set; }

        protected ILogger Logger { get; private set; } = NullLogger.Instance;

        public TrackSegement(FeatureCollection facts) : this(facts, NTSExtensions.DeviationToDistance(10000))
        {
        }

        /// <summary>
        /// can be customized (invented for better testing)
        /// </summary>
        public Func<IFeature, string> ClassificationGetter { get; set; } = (f) => (f.Attributes != null && f.Attributes.Exists(CoordinateClassifier.OutdoorClassAttributeName)) ? f.Attributes[CoordinateClassifier.OutdoorClassAttributeName].ToString() : string.Empty;

        /// <summary>
        /// can be customized (invented for better testing)
        /// </summary>
        public Func<ClassificationProposal, LineString, CoordinateClassification> ClassificationFactory = (prop, sl) => CoordinateClassification.CreateFromProposal(prop, sl);


        public TrackSegement(FeatureCollection facts, double terminateDistance)
        {
            this.Facts = facts ?? throw new ArgumentNullException(nameof(facts));
            this.TerminateDistance = terminateDistance;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">gpx track as LineString (length not restricted, we split inside the method!</param>
        /// <returns></returns>
        public IEnumerable<ClassificationProposal> GetClassificationProposals(LineString input)
        {
            var splitted = input.CreateShortLineStrings();
            foreach (var shortLine in splitted)
            {
                var proposal = this.GetProposal(shortLine);
                Guard.Assert(proposal.LookupKey == shortLine);
                yield return proposal;
            }
        }


        //public IEnumerable<CoordinateClassification>GetClassification(LineString input)
        //{
        //    if (this.ClassificationFactory == null) throw new InvalidOperationException("ClassificationFactory must be defined!");
        //    var splitted = input.CreateShortLineStrings();
        //    foreach (var shortLine in splitted)
        //    {
        //        var proposal = this.GetProposal(shortLine);
        //        var classification = this.ClassificationFactory(proposal, shortLine);
        //        yield return classification;
        //    }
        //}

        public ClassificationProposal GetProposal(Geometry input)
        {
            var facts = this.Facts;

            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (input == null) throw new ArgumentNullException(nameof(input));

            //1. split input into list of Linked

            List<Task<Tuple<IFeature, double, double>>> tasks = new List<Task<Tuple<IFeature, double, double>>>();

            this.Logger.LogDebug($"{nameof(GetProposal)}: {facts.Count} facts for lookup");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            //determine distance to EVERY fact inside the boundingbox!
            foreach (IFeature factFeature in facts)
            {
                Guard.Assert(factFeature.Geometry.GeometryType == "LineString");

                var singleTask = Task.Factory.StartNew<Tuple<IFeature, double, double>>(() =>
                {
                    return factFeature.GetDistance(input, this.TerminateDistance);
                });
                tasks.Add(singleTask);
            }

            this.Logger.LogDebug($"GetDistance Tasks ({tasks.Count}) creation: {sw.Elapsed.ToFormattedDuration()}");
            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            this.Logger.LogDebug($"GetDistance Tasks execution: {sw.Elapsed.ToFormattedDuration()}");

            var notNullResults = tasks.Where(t => t.Result.Item1 != null).Select(t => t.Result).ToList();

            var results = notNullResults.Where(r => r.Item2 < this.TerminateDistance).ToArray(); //ignore all results with distance>TerminateDistance

            ClassificationProposal proposal = new ClassificationProposal(input)
            {
            //    Findings = results.Select(r => new ClassificationFinding(r.Item1.Geometry, r.Item2, r.Item3)).ToList()
            };

            return proposal;



            //var g = results.First().Item1;
            //var angle = AngleUtility.Angle(g.Geometry.Coordinates[0], g.Geometry.Coordinates[1]);
            //if (results.Length == 0)
            //{
            //    return new ClassificationProposal()
            //    {
            //        LookupKey = input,
            //        Classifications = new Dictionary<string, double>()
            //    };
            //}

            //this.Logger.LogTrace($"{results.Length} candidates found");

            //var distinctClasses = results.Select(t => this.ClassificationGetter != null ? this.ClassificationGetter(t.Item1) : string.Empty).Where(c => !string.IsNullOrEmpty(c)).Distinct();

            //Dictionary<string, double> classes = new Dictionary<string, double>();

            //foreach (string c in distinctClasses)
            //{
            //    var resultsForclass = results.Where(r => (this.ClassificationGetter!=null ? this.ClassificationGetter(r.Item1) : string.Empty) == c).ToArray();
            //    if (resultsForclass.Length == 0) continue;
            //    var minValue = resultsForclass.Select(r => r.Item2).Min();
            //    var resultsForMin = resultsForclass.Where(r => r.Item2 == minValue).ToArray();
            //    Tuple<IFeature, double> nearestOne = resultsForMin.First();
            //    classes.Add(c, nearestOne.Item2);
            //}

            //return new ClassificationProposal()
            //{
            //    Classifications = classes,
            //    LookupKey = input,
            //};
        }




        //public CoordinateClassification CreateClassification( Geometry input)
        //{
        //    var facts = this.Facts;
        //    if (facts == null) throw new ArgumentNullException(nameof(facts));
        //    if (input == null) throw new ArgumentNullException(nameof(input));

        //    string description = null;

        //    ClassificationProposal proposal = this.GetProposal(input);

        //    if (proposal == null || proposal.Classifications.Count == 0)
        //    {
        //        description = "no proposal";
        //        return new CoordinateClassification(input, CoordinateClassification.Unknown, "N/A", description);
        //    }

        //    int? trailProposal = proposal.GetDeviationOrDefault(CoordinateClassification.Trail);
        //    int? pavedRoadProposal = proposal.GetDeviationOrDefault(CoordinateClassification.PavedRoad);
        //    int? asphaltedRoadProposal = proposal.GetDeviationOrDefault(CoordinateClassification.AsphaltedRoad);

        //    description = $"TrailDeviation={trailProposal}, PavedDeviation={pavedRoadProposal}, AsphaltedDeviation={asphaltedRoadProposal}";

        //    if ((!trailProposal.HasValue) && (!pavedRoadProposal.HasValue) && (!asphaltedRoadProposal.HasValue))
        //    {
        //        throw new InvalidOperationException("oops");
        //    }

        //    if (trailProposal.HasValue && trailProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
        //    if (pavedRoadProposal.HasValue && pavedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
        //    if (asphaltedRoadProposal.HasValue && asphaltedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);

        //    if ((asphaltedRoadProposal.HasValue) && (pavedRoadProposal.HasValue) && trailProposal.HasValue)
        //    {
        //        int othersMin = Math.Min(asphaltedRoadProposal.Value, pavedRoadProposal.Value);

        //        if (trailProposal.Value < othersMin) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);

        //        if (asphaltedRoadProposal.Value < pavedRoadProposal.Value)
        //        {
        //            return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);
        //        }
        //        else
        //        {
        //            return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
        //        }
        //        throw new InvalidOperationException("decision required");
        //    }

        //    if ((pavedRoadProposal.HasValue) && trailProposal.HasValue)
        //    {
        //        if (trailProposal.Value < pavedRoadProposal + SoloLimit)
        //        {
        //            return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
        //        }
        //        return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
        //    }


        //    if ((asphaltedRoadProposal.HasValue) && trailProposal.HasValue)
        //    {
        //        if (trailProposal.Value < asphaltedRoadProposal + SoloLimit)
        //        {
        //            return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
        //        }
        //        return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);
        //    }

        //    //last exit: without limit
        //    if (trailProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
        //    if (pavedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
        //    if (asphaltedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);

        //    throw new InvalidOperationException("we should never come here");
        //}


        //public Task<TrackSegement> Worker { get; set; }

        //public TrackSegement Previous { get; set; }

        //public TrackSegement Next { get; set; }

        // public LineString Line { get; set; }

        public (LineString InputLineString, LineString ShortLineString) WorkItem { get; set; }

        public CoordinateClassification ResultClassification { get; set; }

        //private IFeature prepare = null;

        //public void PrepareInterpolation(IFeature f)
        //{
        //    this.HasCalculatedValue = false;
        //    prepare = f;
        //}

        //internal void StartInterpolation(FeatureCollection facts)
        //{
        //    Guard.AssertNotNull(prepare);
        //    Guard.AssertNotNull(this.Previous);
        //    Guard.AssertNotNull(this.Next);
        //    Guard.AssertNotNull(this.Previous.Worker);
        //    Guard.AssertNotNull(this.Next.Worker);
        //    Guard.AssertNotNull(this.Previous.HasCalculatedValue);
        //    Guard.AssertNotNull(this.Next.HasCalculatedValue);

        //    this.Worker = Task.Factory.StartNew<TrackSegement>(() =>
        //    {
        //        Task.WaitAll(this.Previous.Worker, this.Next.Worker);

        //        if (this.Previous.ResultClassification.Classification == this.Next.ResultClassification.Classification)
        //        {
        //            this.ResultClassification = new CoordinateClassification(prepare.Geometry, this.Previous.ResultClassification.Classification, this.Previous.ResultClassification.Deviation, "interpolated");
        //            CoordinateClassifier.ApplyAttribute(prepare, this.ResultClassification);

        //        }
        //        else
        //        {
        //            this.ResultClassification = this.Classifier.CreateClassification(facts, this.Line);
        //            CoordinateClassifier.ApplyAttribute(prepare, this.ResultClassification);
        //        }
        //        return this;
        //    });
        //}

        public void StartCalculation(IFeature f, FeatureCollection facts)
        {
            //this.HasCalculatedValue = true;
            //this.Worker = Task.Factory.StartNew<TrackSegement>(() =>
            //{
            //    this.ResultClassification = this.Classifier.CreateClassification(facts, this.Line);
            //    CoordinateClassifier.ApplyAttribute(f, this.ResultClassification);
            //    return this;
            //});
        }

    }

}
