using NetTopologySuite.Geometries;

namespace Trail365.UnitTests
{
    public static class LineCoordinates
    {
        public static readonly Coordinate[] Positive45DegreeLineFrom0 = new Coordinate[] { new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3), new Coordinate(4, 4) };

        public static readonly Coordinate[] Positive315DegreeLineFrom0 = new Coordinate[] { new Coordinate(0, 0), new Coordinate(-1, 1), new Coordinate(-2, 2), new Coordinate(-3, 3), new Coordinate(-4, 4) };

        public static readonly Coordinate[] Positive45DegreeLineFrom10 = new Coordinate[] { new Coordinate(10, 10), new Coordinate(11, 11), new Coordinate(12, 12), new Coordinate(13, 13), new Coordinate(14, 14) };

    }
}
