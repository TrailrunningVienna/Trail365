using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.UnitTests.Builders;
using Trail365.UnitTests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrackAnalyzerTest
    {

        private readonly ITestOutputHelper OutputHelper;

        public TrackAnalyzerTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldAnalyzeTwoIdenticalLines()
        {
            var logger = this.OutputHelper.CreateLogger();
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0,CoordinateClassifier.OutdoorClassAttributeName, CoordinateClassification.Trail).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            TrackAnalyzer analyzer = new TrackAnalyzer(facts).AssignLogger(logger);
            FeatureCollection result = analyzer.Analyze(testLine);
        }
    }
}
