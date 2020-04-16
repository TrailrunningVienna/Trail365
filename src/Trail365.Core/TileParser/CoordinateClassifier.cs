using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public abstract class CoordinateClassifier
    {
        internal static void ApplyAttribute(IFeature feature, CoordinateClassification classification)
        {
            Guard.AssertNotNull(feature);
            Guard.AssertNotNull(classification);

            if (!string.IsNullOrEmpty(classification.Classification))
            {
                if (feature.Attributes == null)
                {
                    feature.Attributes = new AttributesTable();
                }
                feature.Attributes.Add("outdoor_class", classification.Classification);
                feature.Attributes.Add("outdoor_class_quality", classification.Quality);
            }
        }

        public abstract FeatureCollection GetClassification(FeatureCollection input);

        public abstract CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input);

    }
}
