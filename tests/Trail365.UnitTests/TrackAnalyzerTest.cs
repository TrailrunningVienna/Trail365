using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.UnitTests.Builders;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrackAnalyzerTest
    {
        [Fact]
        public void ShouldAnalyzeTwoIdenticalLines()
        {
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0,CoordinateClassifier.OutdoorClassAttributeName, CoordinateClassification.Trail).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            TrackAnalyzer analyzer = new TrackAnalyzer(facts);
            FeatureCollection result = analyzer.Analyze(testLine);

        }
    }
}
