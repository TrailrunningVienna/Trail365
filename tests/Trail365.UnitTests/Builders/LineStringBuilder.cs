using System.Collections.Generic;
using System.Linq;
using System;
using NetTopologySuite.Geometries;

namespace Trail365.UnitTests.Builders
{
    public class LineStringBuilder
    {
        private readonly GeometryFactory Factory = new GeometryFactory();

        internal Func<IEnumerable<Coordinate>> CoordinatesGetter
        { get; set; }


        public LineString Build()
        {
            Coordinate[] coordinates = this.CoordinatesGetter != null ? this.CoordinatesGetter().ToArray() : new Coordinate[] { };
            return this.Factory.CreateLineString(coordinates);

        }

        public static LineStringBuilder CreateLineString(params Coordinate[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException(nameof(coordinates));
            }
            return new LineStringBuilder()
            {
                CoordinatesGetter = () => coordinates
            };
        }
    }
}
