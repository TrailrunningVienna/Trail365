using System.Linq;
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

        [Fact]
        public void ShouldAnalyzeTwoIdenticalLines()
        {
            var logger = this.OutputHelper.CreateLogger();
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0, CoordinateClassifier.OutdoorClassAttributeName, CoordinateClassification.Trail).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            TrackAnalyzer analyzer = new TrackAnalyzer(facts).AssignLogger(logger);
            FeatureCollection result = analyzer.Analyze(testLine);
        }

        [Fact]
        public void ShouleAnalyzeVTRLight()
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
            TrackAnalyzer analyzer = new TrackAnalyzer(facts, NTSExtensions.DeviationToDistance(10000)).AssignLogger(logger);
            var result = analyzer.Analyze(testTrack);

            //assert results
            Assert.Equal(238, result.Count);
            var trailShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Trail).Count();
            var pavedShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.PavedRoad).Count();
            var unknownShortLines = result.Where(f => $"{f.Attributes[CoordinateClassifier.OutdoorClassAttributeName]}" == CoordinateClassification.Unknown).Count();

            Assert.Equal(91, trailShortLines);
            Assert.Equal(113, pavedShortLines);
            Assert.Equal(0, unknownShortLines);

        }
    }
}
