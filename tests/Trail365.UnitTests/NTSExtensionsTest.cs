using NetTopologySuite.Geometries;
using Xunit;
using NetTopologySuite.Features;
using TrackExplorer.Core;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class NTSExtensionsTest
    {

        [Fact]
        public void Empty()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);
            var pt = Geometry.DefaultFactory.CreatePoint(new Coordinate(0, 1));
            var result = feature.GetDistance(pt, 0.1); //0.1 reduces effort but not searchdepth/result!
            Assert.Equal(0.0, result.Item2);
            Assert.Null(result.Item1);
        }


        [Fact]
        public void TerminateDistance()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 1) };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);
            var pt = Geometry.DefaultFactory.CreatePoint(new Coordinate(0, 1));
            var result = feature.GetDistance(pt, 0.1); //0.1 reduces effort but not searchdepth/result!
            Assert.Equal(1, result.Item2);
        }


        [Fact]
        public void ShouldReturnDistance_1()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 1) };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);
            var pt = Geometry.DefaultFactory.CreatePoint(new Coordinate(0, 1));

            var result = feature.GetDistance(pt, 0);
            Assert.Equal(1, result.Item2);



        }
    }
}
