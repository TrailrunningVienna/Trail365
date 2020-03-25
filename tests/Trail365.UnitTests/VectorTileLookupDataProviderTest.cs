using TrackExplorer.Core;
using Trail365.Web.Tasks;
using Xunit;

namespace Trail365.UnitTests
{
    public class VectorTileLookupDataProviderTest
    {
        [Fact]

        public void ShouldDecodeMVTIntoFeatureCollection()
        {
            //Arrange
            byte[] gpxFileContent = System.IO.File.ReadAllBytes(Helper.GetExtendedRultGpxPath());
            var gpxFileFeatureCollection = TrailAnalyzerTask.ConvertToFeatureCollection(gpxFileContent);
            var gpxFileBoundaries = gpxFileFeatureCollection.GetBoundaries();
            var mvtLocationUri = MvtTiles.GetDirectoryUri();
            string url = mvtLocationUri.ToString(); //@"https://trex.blob.core.windows.net/tiles";

            VectorTileLookupDataProvider provider = new VectorTileLookupDataProvider(url, "outdoor", 12);

            //Act
            var featuresforEnvelope = provider.GetClassifiedMapFeatures(gpxFileBoundaries.Envelope);

            //Assert
            Assert.Equal(6241, featuresforEnvelope.Count);
        }
    }
}
