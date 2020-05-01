using NetTopologySuite.Features;
using Trail365.Seeds;
using Trail365.UnitTests.Utils;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
namespace Trail365.IntegrationTests
{
    public class LookupCoordinateClassifierTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public LookupCoordinateClassifierTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public static readonly string TileSourceUrl = @"https://trex.blob.core.windows.net/tiles";
        public static readonly string outputFolder = @"m:\work";

        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        public void ShouldClassifyVTRLight(int zoomLevel)
        {
            var logger = this.OutputHelper.CreateLogger();
            byte[] gpxFileContent = System.IO.File.ReadAllBytes(GpxTracks.VTRLight);
            var gpxFileFeatureCollection = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
            var gpxFileBoundaries = gpxFileFeatureCollection.GetBoundaries();

            VectorTileLookupDataProvider provider = new VectorTileLookupDataProvider(TileSourceUrl, "outdoor", zoomLevel);
            provider.AssignLogger(logger);

            CoordinateClassifier classifier = new LookupCoordinateClassifier(provider);
            classifier.AssignLogger(logger);

            FeatureCollection inputData = TrailExtender.ConvertToFeatureCollection(gpxFileContent); //NOT simplified, we need detailed data here
            FeatureCollection classifiedData = classifier.GetClassification(inputData);
            classifiedData.SerializeFeatureCollectionIntoGeoJson(System.IO.Path.Combine(outputFolder, $"{nameof(ShouldClassifyVTRLight)}_{zoomLevel}.json"));
        }

        [Theory]
        [InlineData(10)]
        //[InlineData(11)]
        //[InlineData(12)]
        //[InlineData(13)]
        //[InlineData(14)]
        [InlineData(15)]
        public void ShouldClassifyWanderweg41(int zoomLevel)
        {
            var logger = this.OutputHelper.CreateLogger();
            byte[] gpxFileContent = System.IO.File.ReadAllBytes(GpxTracks.Wanderweg41);

            var gpxFileFeatureCollection = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
            var gpxFileBoundaries = gpxFileFeatureCollection.GetBoundaries();

            VectorTileLookupDataProvider provider = new VectorTileLookupDataProvider(TileSourceUrl, "outdoor", zoomLevel);
            provider.AssignLogger(logger);

            CoordinateClassifier classifier = new LookupCoordinateClassifier(provider);
            classifier.AssignLogger(logger);

            FeatureCollection inputData = TrailExtender.ConvertToFeatureCollection(gpxFileContent); //NOT simplified, we need detailed data here
            FeatureCollection classifiedData = classifier.GetClassification(inputData);
            classifiedData.SerializeFeatureCollectionIntoGeoJson(System.IO.Path.Combine(outputFolder, $"{nameof(ShouldClassifyWanderweg41)}_{zoomLevel}.json"));
        }

    }
}
