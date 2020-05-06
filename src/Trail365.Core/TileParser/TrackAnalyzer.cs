using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{


    public class TrackAnalyzer
    {
        public TrackAnalyzerSettings Settings { get; private set; } = new TrackAnalyzerSettings() { TerminateDistance = TerminateDistanceDefault };

        private readonly FeatureCollection MapFacts;

        public static readonly double TerminateDistanceDefault = NTSExtensions.DeviationToDistance(10000);
        protected internal ILogger Logger { get; private set; } = NullLogger.Instance;

        public TrackAnalyzer AssignLogger(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            this.Logger = logger;
            return this;
        }

        public TrackAnalyzer(FeatureCollection mapFacts) : this(mapFacts, TerminateDistanceDefault)
        {

        }

        public static List<LineSegmentProposal> CalculateFindings(Geometry fact, string mapClass, Geometry shortLine, TrackAnalyzerSettings settings, CancellationToken cancellationToken, ILogger logger)
        {
            //1. split input into list of Linked
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var result = fact.GetLineSegmentProposalOrDefault(mapClass, shortLine, settings, logger);
            sw1.Stop();
            logger.LogTrace($"[{nameof(CalculateFindings)}] Single lookup execution: {sw1.Elapsed.ToFormattedDuration()}");
            var rawResults = new List<LineSegmentProposal>();
            if (result != null)
            {
                rawResults.Add(result);
            }
            return rawResults;
        }

        public static List<LineSegmentProposal> CalculateFindings(FeatureCollection facts, Geometry shortLine, TrackAnalyzerSettings settings, CancellationToken cancellationToken, ILogger logger, Func<IFeature, string> classGetter)
        {
            //1. split input into list of Linked
            logger.LogDebug($"[{nameof(CalculateFindings)}] {facts.Count} facts for lookup");
            List<Task<LineSegmentProposal>> tasks = new List<Task<LineSegmentProposal>>();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            //determine distance to EVERY fact inside the boundingbox!
            foreach (var factFeature in facts)
            {
                var factGeometry = factFeature.Geometry;
                var mapClass = classGetter(factFeature);
                cancellationToken.ThrowIfCancellationRequested();
                var singleTask = Task.Factory.StartNew<LineSegmentProposal>(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return factGeometry.GetLineSegmentProposalOrDefault(mapClass, shortLine, settings, logger);
                }, cancellationToken);
                tasks.Add(singleTask);
            }
            logger.LogDebug($"[{nameof(CalculateFindings)}] {tasks.Count} tasks created in {sw.Elapsed.ToFormattedDuration()}");
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            logger.LogDebug($"[{nameof(CalculateFindings)}] Tasks executed in {sw.Elapsed.ToFormattedDuration()}");
            var notNullResults = tasks.Where(t => t.Result != null).Select(t => t.Result).ToList();
            return notNullResults;
        }

        public FeatureCollection Analyze(LineString track, CancellationToken cancellation = default)
        {
            var splitted = track.CreateShortLineStrings();
            return Analyze(splitted, cancellation);
        }

        public FeatureCollection Analyze(FeatureCollection track, CancellationToken cancellation = default)
        {
            var splitted = track.GetLineStrings().CreateShortLineStrings();
            return Analyze(splitted, cancellation);
        }

        internal FeatureCollection Analyze(IEnumerable<LineString> shortStrings, CancellationToken cancellation)
        {
            this.Logger.LogDebug($"Start track analysis with max deviation={CoordinateClassification.ToDeviation(this.Settings.TerminateDistance)} and max angle diff={CoordinateClassification.ToAngleDiff(this.Settings.MaximumAngleDiff)}");
            if (shortStrings == null) throw new ArgumentNullException(nameof(shortStrings));

            //Idea: collect Proposals using smaller restrictions that are resulting in more proposals
            //refine proposals into classifications with the stronger settings, but print out the filtered out proposals for tracing.

            var weakSettings = this.Settings.ToWeaker();

            List<ClassificationProposal> proposals = new List<ClassificationProposal>();
            ClassificationProposal previous = null;
            foreach (var shortLine in shortStrings)
            {
                var current = this.GetProposal(shortLine, previous, weakSettings);
                Guard.AssertNotNull(current, "Each shortline needs a proposal. It may be empty/not classified!");
                Guard.Assert(current.LookupKey == shortLine);
                proposals.Add(current);
                previous = current;
            }

            var result = new FeatureCollection();

            GeometryFactory factory = new GeometryFactory();

            this.ClassificationFactory = (prop, sl) =>
            {
                return CoordinateClassification.CreateFromProposal(prop, sl, this.Settings, this.Logger);
            };

            foreach (var proposal in proposals)
            {
                var classification = this.ClassificationFactory(proposal, proposal.LookupKey as LineString);
                var line = factory.CreateLineString(proposal.LookupKey.Coordinates);
                var feature = new Feature(line, attributes: null);
                CoordinateClassifier.ApplyAttribute(feature, classification);
                result.Add(feature);
            }

            return result;
        }

        public TrackAnalyzer(FeatureCollection mapFacts, double terminateDistance)
        {
            this.MapFacts = mapFacts ?? throw new ArgumentNullException(nameof(mapFacts));
            this.Settings.TerminateDistance = terminateDistance;
        }

        /// <summary>
        /// can be customized (invented for better testing)
        /// </summary>
        public Func<IFeature, string> ClassificationGetter { get; set; } = (f) => (f.Attributes != null && f.Attributes.Exists(CoordinateClassifier.OutdoorClassAttributeName)) ? f.Attributes[CoordinateClassifier.OutdoorClassAttributeName].ToString() : string.Empty;

        /// <summary>
        /// can be customized (invented for better testing)
        /// </summary>
        public Func<ClassificationProposal, LineString, CoordinateClassification> ClassificationFactory { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="input">gpx track as LineString (length not restricted, we split inside the method!</param>
        ///// <returns></returns>
        //public IEnumerable<ClassificationProposal> GetClassificationProposals(LineString input)
        //{
        //    var splitted = input.CreateShortLineStrings();
        //    foreach (var shortLine in splitted)
        //    {
        //        var proposal = this.GetProposal(shortLine, null, this.Settings);
        //        Guard.Assert(proposal.LookupKey == shortLine);
        //        yield return proposal;
        //    }
        //}

        private static string GetOutdoorClass(IFeature feature)
        {
            if (feature.Attributes == null) return string.Empty;
            return $"{feature.Attributes.GetOptionalValue(CoordinateClassifier.OutdoorClassAttributeName)}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousOrDefault"></param>
        /// <returns>there must be always a proposal, also if there are no findings!</returns>
        public ClassificationProposal GetProposal(Geometry input, ClassificationProposal previousOrDefault, TrackAnalyzerSettings settings)
        {
            var facts = this.MapFacts;

            var ct = CancellationToken.None;

            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (input == null) throw new ArgumentNullException(nameof(input));

            List<LineSegmentProposal> findings = null;

            if (previousOrDefault != null && previousOrDefault.Proposals.Count > 0)
            {

                //cheap lookup
                var candidates = previousOrDefault.Proposals.Select(ls => new Tuple<Geometry, string>(ls.Owner, ls.Classification)).Distinct().ToList();
                List<LineSegmentProposal> findingCollector = new List<LineSegmentProposal>();

                var strongSettings = settings.ToStronger(); //use stronger settings if we reduce the search on the current fact!
                foreach (var candidate in candidates)
                {
                    var subFindings = CalculateFindings(candidate.Item1, candidate.Item2, input, strongSettings, ct, this.Logger);
                    findingCollector.AddRange(subFindings);
                }

                if (findingCollector.Count > 0)
                {
                    findings = findingCollector;
                }
            }

            if (findings == null)
            {
                //expensive lookup
                List<LineSegmentProposal> findingCollector = CalculateFindings(this.MapFacts, input, settings, ct, this.Logger, GetOutdoorClass);
                findings = findingCollector;
            }

            ClassificationProposal proposal = new ClassificationProposal(input, findings);
            return proposal;
        }

        public (LineString InputLineString, LineString ShortLineString) WorkItem { get; set; }

        public CoordinateClassification ResultClassification { get; set; }

    }

}
