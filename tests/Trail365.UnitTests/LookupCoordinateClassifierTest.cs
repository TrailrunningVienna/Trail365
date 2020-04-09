using Xunit;
using Trail365.Web.Tasks;
using Trail365.Seeds;

namespace Trail365.UnitTests
{

    public class LookupCoordinateClassifierTest
    {
        private readonly LookupDataProvider LookupDataProvider;
        private readonly NetTopologySuite.Features.FeatureCollection TestData;

        public LookupCoordinateClassifierTest()
        {
            //do resource intensive preparations here, to see more realistic duration times for single tests!
            var mvtLocationUri = MvtTiles.GetDirectoryUri();
            string url = mvtLocationUri.ToString(); //@"https://trex.blob.core.windows.net/tiles";
            this.LookupDataProvider = new VectorTileLookupDataProvider(url, "outdoor", 12);
            byte[] gpxFileContent = System.IO.File.ReadAllBytes(Helper.GetExtendedRultGpxPath());
            this.TestData = TrailExtender.ConvertToFeatureCollection(gpxFileContent);
            var gpxFileBoundaries = this.TestData.GetBoundaries();
            this.LookupDataProvider.GetClassifiedMapFeatures(gpxFileBoundaries.Envelope); //cache warmup
        }

        //[Fact]
        //public void ShouldClassifyExtendedRult()
        //{

        //    CoordinateClassifier classifier = new LookupCoordinateClassifier(this.LookupDataProvider);
        //    //Act
        //    var classifiedData = classifier.GetClassification(this.TestData);
        //}


        //[Fact]
        //public void ShouldClassifyRoute()
        //{

        //    byte[] gpxFileContent = System.IO.File.ReadAllBytes(GpxTracks.SharedRoute);
        //    var td = TrailAnalyzerTask.ConvertToFeatureCollection(gpxFileContent);
        //    var gpxFileBoundaries = td.GetBoundaries();


        //    CoordinateClassifier classifier = new LookupCoordinateClassifier(this.LookupDataProvider);
        //    //Act
        //    var classifiedData = classifier.GetClassification(td);
        //}


    }
}
