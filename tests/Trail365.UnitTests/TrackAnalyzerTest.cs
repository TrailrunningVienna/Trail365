using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Seeds;
using Trail365.TileParser;
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

        [Theory]
        [InlineData(1, System.Math.PI, 1)]
        [InlineData(2, System.Math.PI, 2)]
        [InlineData(10, System.Math.PI, 4)]

        [InlineData(1, System.Math.PI / 2, 1)]
        [InlineData(2, System.Math.PI / 2, 2)]
        [InlineData(10, System.Math.PI / 2, 4)]


        [InlineData(1, System.Math.PI / 4, 0)]
        [InlineData(2, System.Math.PI / 4, 0)]
        [InlineData(10, System.Math.PI / 4, 0)]

        public void ShouldAnalyze_V_Lines(double terminateDistance, double maxAngle, int expected)
        {
            var logger = this.OutputHelper.CreateLogger();
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0, CoordinateClassifier.OutdoorClassAttributeName, CoordinateClassification.Trail).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive315DegreeLineFrom0).Build();

            TrackAnalyzer analyzer = new TrackAnalyzer(facts, terminateDistance).AssignLogger(logger);
            analyzer.Settings.MaximumAngleDiff = maxAngle;

            FeatureCollection result = analyzer.Analyze(testLine);

            var trailShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Trail).Count();
            Assert.Equal(expected, trailShortLines);
        }


        [Fact]
        public void ShouldAnalyzeTwoIdenticalLines()
        {
            var logger = this.OutputHelper.CreateLogger();
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0, CoordinateClassifier.OutdoorClassAttributeName, CoordinateClassification.Trail).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            TrackAnalyzer analyzer = new TrackAnalyzer(facts).AssignLogger(logger);
            FeatureCollection result = analyzer.Analyze(testLine);
            var trailShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Trail).Count();
            Assert.Equal(4, trailShortLines);

        }

        [Theory]
        [InlineData(System.Math.PI, (double)10000 / NTSExtensions.DeviationFactor, 90, 120, 0)]
        [InlineData(System.Math.PI / 4, (double)10000 / NTSExtensions.DeviationFactor, 94, 111, 9)]
        [InlineData(System.Math.PI / 4, (double)1000 / NTSExtensions.DeviationFactor, 41, 50, 135)]
        public void ShouleAnalyzeVTRLight(double maxAngle, double maxDistance, int expectedTrails, int expectedPaved, int expectedUnknown)
        {
            var logger = this.OutputHelper.CreateLogger();

            //arrange: load and verify test track
            FeatureCollection testTrack = TrailExtender.ConvertToFeatureCollection(GpxTracks.VTRLight);
            Assert.Equal(239, testTrack[0].Geometry.Coordinates.Length);

            //arrange: load and verify facts!
            var tileInfo = new TileInfo(2233, 1419, 12);
            FeatureCollection facts = VectorTileLayerExtensions.CreateFrom(MvtTiles.Tile_2233_1419, tileInfo, true);
            Assert.Equal(6833, facts.Count);
            var envelope = facts.GetBoundaries().Envelope;
            Assert.Equal(0.0072660361635144, envelope.Boundary.Length);
            var trailFeatures = facts.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Trail).Count();
            var asphaltFeatures = facts.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.AsphaltedRoad).Count();
            Assert.Equal(trailFeatures, 948);
            Assert.Equal(asphaltFeatures, 5442);

            //act: analyze track using facts
            TrackAnalyzer analyzer = new TrackAnalyzer(facts).AssignLogger(logger);
            analyzer.Settings.MaximumAngleDiff = maxAngle;
            analyzer.Settings.TerminateDistance = maxDistance;
            var result = analyzer.Analyze(testTrack);

            //assert results
            Assert.Equal(238, result.Count);
            var trailShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Trail).Count();
            var pavedShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.PavedRoad).Count();
            var unknownShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Unknown).Count();

            Assert.Equal(expectedTrails, trailShortLines);
            Assert.Equal(expectedPaved, pavedShortLines);
            Assert.Equal(expectedUnknown, unknownShortLines);

        }
    }
}
