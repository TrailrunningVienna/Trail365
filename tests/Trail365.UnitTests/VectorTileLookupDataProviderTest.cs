using NetTopologySuite.Features;
using Trail365.Web.Tasks;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class VectorTileLookupDataProviderTest
    {
        //[Fact]
        //public void Shouldxxx()
        //{
        //    //Arrange
        //    byte[] gpxFileContent = System.IO.File.ReadAllBytes(Helper.GetExtendedRultGpxPath());
        //    //var gpxFileFeatureCollection = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
        //    //var gpxFileBoundaries = gpxFileFeatureCollection.GetBoundaries();
        //    //var mvtLocationUri = MvtTiles.GetDirectoryUri();
        //    string url = @"https://trex.blob.core.windows.net/tiles";
        //    VectorTileLookupDataProvider provider = new VectorTileLookupDataProvider(url, "outdoor", 14);
        //    CoordinateClassifier classifier = new LookupCoordinateClassifier(provider);

        //    //var buffer = await contentDownloader.GetFromUriAsync(new Uri(trail.GpxBlob.Url), cancellationToken);
        //    //cancellationToken.ThrowIfCancellationRequested();

        //    FeatureCollection inputData = TrailExtender.ConvertToFeatureCollection(gpxFileContent); //NOT simplified, we need detailed data here


        //    FeatureCollection classifiedData = classifier.GetClassification(inputData);

        //}


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
