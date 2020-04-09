using Trail365.Web.Tasks;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class VectorTileLookupDataProviderTest
    {
        [Fact]

        public void ShouldDecodeMVTIntoFeatureCollection()
        {
            //Arrange
            byte[] gpxFileContent = System.IO.File.ReadAllBytes(Helper.GetExtendedRultGpxPath());
            var gpxFileFeatureCollection = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
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
