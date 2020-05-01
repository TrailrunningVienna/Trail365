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


        [Fact]
        public void Shouldxxx()
        {
            var logger = this.OutputHelper.CreateLogger();

            //byte[] gpxFileContent = System.IO.File.ReadAllBytes(GpxTracks.Wanderweg41);
            byte[] gpxFileContent = System.IO.File.ReadAllBytes(GpxTracks.VTRLight);

            var gpxFileFeatureCollection = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
            var gpxFileBoundaries = gpxFileFeatureCollection.GetBoundaries();

            VectorTileLookupDataProvider provider = new VectorTileLookupDataProvider(TileSourceUrl, "outdoor", 12);
            provider.AssignLogger(logger);

            CoordinateClassifier classifier = new LookupCoordinateClassifier(provider);
            classifier.AssignLogger(logger);

            FeatureCollection inputData = TrailExtender.ConvertToFeatureCollection(gpxFileContent); //NOT simplified, we need detailed data here
            FeatureCollection classifiedData = classifier.GetClassification(inputData);
            classifiedData.SerializeFeatureCollectionIntoGeoJson(@"c:\work\classifiedOutput.Wanderweg41.json");

        }
    }
}
