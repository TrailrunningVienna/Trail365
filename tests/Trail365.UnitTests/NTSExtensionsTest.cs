using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.UnitTests.Builders;
using Xunit;

namespace Trail365.UnitTests
{

    [Trait("Category", "BuildVerification")]
    public class NTSExtensionsTest
    {

        [Fact]
        public void ShouldNormalizeAngle90()
        {
            var r1 = NTSExtensions.NormalizedAngle(new Coordinate(1, 1), new Coordinate(1, 2));
            var r2 = NTSExtensions.NormalizedAngle(new Coordinate(1, 2), new Coordinate(1, 1));
            Assert.Equal(r1, r2);

            var r3 = NTSExtensions.NormalizedAngle(new Coordinate(-1, -1), new Coordinate(-1, -2));
            var r4 = NTSExtensions.NormalizedAngle(new Coordinate(-1, -2), new Coordinate(-1, -1));
            Assert.Equal(r3, r4);

        }

        [Fact]
        public void ShouldNormalizeAngle0()
        {
            var r1 = NTSExtensions.NormalizedAngle(new Coordinate(1, 1), new Coordinate(2, 1));
            var r2 = NTSExtensions.NormalizedAngle(new Coordinate(2, 1), new Coordinate(1, 1));
            Assert.Equal(r1, r2);

            r1 = NTSExtensions.NormalizedAngle(new Coordinate(-1, -1), new Coordinate(-2, -1));
            r2 = NTSExtensions.NormalizedAngle(new Coordinate(-2, -1), new Coordinate(-1, -1));
            Assert.Equal(r1, r2);

        }


        [Fact]
        public void ShouldNormalizeAngle45()
        {
            var r1 = NTSExtensions.NormalizedAngle(new Coordinate(1, 1), new Coordinate(2, 2));
            var r2 = NTSExtensions.NormalizedAngle(new Coordinate(2, 2), new Coordinate(1, 1));
            Assert.Equal(r1, r2);

            r1 = NTSExtensions.NormalizedAngle(new Coordinate(-1, -1), new Coordinate(-2, -2));
            r2 = NTSExtensions.NormalizedAngle(new Coordinate(-2, -2), new Coordinate(-1, -1));
            Assert.Equal(r1, r2);

        }

        [Fact]
        public void ShouldBeTheSame()
        {
            LineString factLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0[0], LineCoordinates.Positive45DegreeLineFrom0[1]).Build();
            var result = NTSExtensions.GetDistance(new Feature(factLine, null), testLine, 10);
            Assert.NotNull(result.Item1);
            Assert.Equal(result.Item2, 0);
        }

        [Fact]
        public void ShouldHaveNoResult()
        {
            LineString factLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom10[0], LineCoordinates.Positive45DegreeLineFrom10[1]).Build();
            var result = NTSExtensions.GetDistance(new Feature(factLine, null), testLine, 0.1);
            Assert.Null(result.Item1);
            Assert.True(result.Item2 > 0.1);
        }

        [Fact]
        public void Angle_1()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { new Coordinate(1, 4), new Coordinate(1, 3), new Coordinate(1, 2), new Coordinate(1, 1), new Coordinate(2, 1), new Coordinate(3, 1), new Coordinate(4, 1) };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);
            var testLine = Geometry.DefaultFactory.CreateLineString(new Coordinate[] { new Coordinate(-1, -1), new Coordinate(-2, -2) });

            var result1 = feature.GetDistance(testLine, 0);
            Assert.Equal(AngleUtility.PiOver2, result1.Item3);
        }

        [Fact]
        public void Angle_2()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { new Coordinate(4, 4), new Coordinate(4, 3), new Coordinate(4, 2), new Coordinate(3, 2), new Coordinate(3, 3), new Coordinate(3, 4) };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);

            var testLine = Geometry.DefaultFactory.CreateLineString(new Coordinate[] { new Coordinate(3, 2), new Coordinate(4, 2) });

            var result1 = feature.GetDistance(testLine, 0);
            Assert.Equal(0, result1.Item2);
            Assert.Equal(0, result1.Item3);
            Assert.NotNull(result1.Item1);
        }

        [Fact]
        public void TerminateDistance()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 1) };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);
            var testLine = Geometry.DefaultFactory.CreateLineString(new Coordinate[] { new Coordinate(0, 0), new Coordinate(0, 1) });


            var result1 = feature.GetDistance(testLine, 2);
            Assert.Equal(1, result1.Item2);
            Assert.NotNull(result1.Item1);

            var result = feature.GetDistance(testLine, 0.1); //0.1 reduces effort but not searchdepth/result!
            Assert.Equal(1, result.Item2);
            Assert.Null(result.Item1);
        }

        [Fact]
        public void ShouldReturnDistance_1()
        {
            var lookups = new FeatureCollection();
            Coordinate[] points = new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 1) };
            LineString ls = new LineString(points);
            var feature = new Feature(ls, null);
            var testLine = Geometry.DefaultFactory.CreateLineString(new Coordinate[] { new Coordinate(0, 0), new Coordinate(0, 1) });
            var result = feature.GetDistance(testLine, 0);
            Assert.Equal(1, result.Item2);
        }
    }
}
