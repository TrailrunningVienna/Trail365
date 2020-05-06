using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public sealed class LookupCoordinateClassifier : CoordinateClassifier
    {

        private readonly LookupDataProvider LookupDataProvider;

        public override FeatureCollection GetClassification(FeatureCollection input)
        {
            var facts = this.LookupDataProvider.GetClassifiedMapFeatures(input.GetBoundaries().Envelope);
            TrackAnalyzer analyzer = new TrackAnalyzer(facts);
            analyzer.Settings.TerminateDistance = NTSExtensions.DeviationToDistance(2000);
            analyzer.Settings.MaximumAngleDiff = AngleUtility.PiOver4;

            analyzer.AssignLogger(this.Logger);
            var result = analyzer.Analyze(input);
            //merging skipped because we need details for each ShortLine
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facts">geoJson file with knowledge about the area</param>
        public LookupCoordinateClassifier(LookupDataProvider lookupDataProvider)
        {
            this.LookupDataProvider = lookupDataProvider ?? throw new ArgumentNullException(nameof(lookupDataProvider));
        }
    }
}
