using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public class TrackAnalyzer
    {
        private readonly FeatureCollection MapFacts;
        public double TerminateDistance { get; private set; }

        protected ILogger Logger { get; private set; } = NullLogger.Instance;

        public TrackAnalyzer(FeatureCollection mapFacts) : this(mapFacts, NTSExtensions.DeviationToDistance(10000))
        {

        }

        public static List<LineSegmentProposal> CalculateFindings(Geometry fact, string mapClass, Geometry shortLine, TrackAnalyzerSettings settings, CancellationToken cancellationToken, ILogger logger)
        {
            //1. split input into list of Linked
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var result = fact.GetLineSegmentProposalOrDefault(mapClass, shortLine, settings.TerminateDistance,logger);
            sw1.Stop();
            //logger.LogDebug($"{nameof(CalculateFindings)} single lookup execution: {sw1.Elapsed.ToFormattedDuration()}");
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
            logger.LogDebug($"{nameof(CalculateFindings)}: {facts.Count} facts for lookup");
            List<Task<LineSegmentProposal>> tasks = new List<Task<LineSegmentProposal>>();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            //determine distance to EVERY fact inside the boundingbox!
            foreach (var factFeature in facts)
            {
                var factGeometry = factFeature.Geometry;
                var mapClass = classGetter(factFeature);
                cancellationToken.ThrowIfCancellationRequested();
                //Guard.Assert(factGeometry.GeometryType == "LineString");
                if (factGeometry.GeometryType == "MultiLineString")
                {
                    System.Diagnostics.Debug.WriteLine("");
                }
                var singleTask = Task.Factory.StartNew<LineSegmentProposal>(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return factGeometry.GetLineSegmentProposalOrDefault(mapClass, shortLine, settings.TerminateDistance, logger);
                }, cancellationToken);
                tasks.Add(singleTask);
            }
            logger.LogDebug($"{nameof(CalculateFindings)} Tasks ({tasks.Count}) creation: {sw.Elapsed.ToFormattedDuration()}");
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            logger.LogDebug($"{nameof(CalculateFindings)} Tasks execution: {sw.Elapsed.ToFormattedDuration()}");
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
            if (shortStrings == null) throw new ArgumentNullException(nameof(shortStrings));

            List<ClassificationProposal> proposals = new List<ClassificationProposal>();

            ClassificationProposal previous = null;

            foreach (var shortLine in shortStrings)
            {
                var current = this.GetProposal(shortLine, previous);
                Guard.AssertNotNull(current, "Each shortline needs a proposal. It may be empty/not classified!");
                Guard.Assert(current.LookupKey == shortLine);
                proposals.Add(current);
                previous = current;
            }

            var result = new FeatureCollection();

            GeometryFactory factory = new GeometryFactory();

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
            this.TerminateDistance = terminateDistance;
        }


        /// <summary>
        /// can be customized (invented for better testing)
        /// </summary>
        public Func<IFeature, string> ClassificationGetter { get; set; } = (f) => (f.Attributes != null && f.Attributes.Exists(CoordinateClassifier.OutdoorClassAttributeName)) ? f.Attributes[CoordinateClassifier.OutdoorClassAttributeName].ToString() : string.Empty;

        /// <summary>
        /// can be customized (invented for better testing)
        /// </summary>
        public Func<ClassificationProposal, LineString, CoordinateClassification> ClassificationFactory = (prop, sl) => CoordinateClassification.CreateFromProposal(prop, sl);

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
                var proposal = this.GetProposal(shortLine, null);
                Guard.Assert(proposal.LookupKey == shortLine);
                yield return proposal;
            }
        }

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
        public ClassificationProposal GetProposal(Geometry input, ClassificationProposal previousOrDefault)
        {
            var facts = this.MapFacts;
            var settings = new TrackAnalyzerSettings() { TerminateDistance = this.TerminateDistance };
            var ct = CancellationToken.None;

            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (input == null) throw new ArgumentNullException(nameof(input));

            List<LineSegmentProposal> findings = null;

            if (previousOrDefault != null)
            {
                //cheap lookup
                var candidates = previousOrDefault.LinkedLineSegments.Select(ls => new Tuple<Geometry, string>(ls.Owner, ls.Classification)).Distinct().ToList();
                List<LineSegmentProposal> findingCollector = new List<LineSegmentProposal>();
                foreach (var candidate in candidates)
                {
                    var subFindings = CalculateFindings(candidate.Item1, candidate.Item2, input, settings, ct, this.Logger);
                    findingCollector.AddRange(subFindings);
                }

                //TODO add quality checks here!
                if (findingCollector.Count > 0)
                {
                    findings = findingCollector;
                }
            }

            if (findings == null)
            {
                //expensive lookup
                findings = CalculateFindings(this.MapFacts, input, settings, ct, this.Logger, GetOutdoorClass);
            }

            ClassificationProposal proposal = new ClassificationProposal(input)
            {
                LinkedLineSegments = findings
            };
            return proposal;
        }

        public (LineString InputLineString, LineString ShortLineString) WorkItem { get; set; }

        public CoordinateClassification ResultClassification { get; set; }

    }

}