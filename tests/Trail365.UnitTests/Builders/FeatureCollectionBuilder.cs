using System;
using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Trail365.UnitTests.Builders
{
    public class FeatureCollectionBuilder
    {
        private readonly GeometryFactory Factory = new GeometryFactory();
        internal Func<GeometryFactory, IEnumerable<Feature>> Features { get; set; }
        public FeatureCollection Build()
        {
            FeatureCollection fc = new FeatureCollection();
            if (this.Features != null)
            {
                foreach (var f in this.Features(this.Factory))
                {
                    fc.Add(f);
                }
            }
            return fc;
        }


        public static FeatureCollectionBuilder CreateLineString(LineStringBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return new FeatureCollectionBuilder()
            {
                Features = (factory) =>
                {
                    return new Feature[] { new Feature(builder.Build(), attributes: null) };
                }
            };
        }


        public static FeatureCollectionBuilder CreateLineString(Coordinate[] coordinates)
        {
            return new FeatureCollectionBuilder()
            {
                Features = (factory) =>
                {
                    return new Feature[] { new Feature(factory.CreateLineString(coordinates), attributes: null) };
                }
            };
        }

        public static FeatureCollectionBuilder CreateLineString(Coordinate[] coordinates, string attributeName, object attributeValue)
        {
            return new FeatureCollectionBuilder()
            {
                Features = (factory) =>
                {
                    AttributesTable attribs = new AttributesTable();
                    attribs.Add(attributeName, attributeValue);
                    return new Feature[] { new Feature(factory.CreateLineString(coordinates), attribs) };
                }
            };
        }


    }
}
