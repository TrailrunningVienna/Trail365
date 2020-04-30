using NetTopologySuite.Features;
using Trail365.Seeds;
using Trail365.UnitTests.Utils;
using Xunit;
using Xunit.Abstractions;

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


        [Fact]
        public void Shouldxxx()
        {
            var lgp = new XunitLoggerProvider(OutputHelper);

            byte[] gpxFileContent = System.IO.File.ReadAllBytes(GpxTracks.Wanderweg41);
            var gpxFileFeatureCollection = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
            var gpxFileBoundaries = gpxFileFeatureCollection.GetBoundaries();
            VectorTileLookupDataProvider provider = new VectorTileLookupDataProvider(TileSourceUrl, "outdoor", 12);
            CoordinateClassifier classifier = new LookupCoordinateClassifier(provider);
            classifier.AssignLogger(lgp.CreateLogger(classifier.GetType().Name));
            FeatureCollection inputData = TrailExtender.ConvertToFeatureCollection(gpxFileContent); //NOT simplified, we need detailed data here
            FeatureCollection classifiedData = classifier.GetClassification(inputData);
            classifiedData.SerializeFeatureCollectionIntoGeoJson(@"c:\work\classifiedOutput.Wanderweg41.json");
        }
    }
}
