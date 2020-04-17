using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public abstract class CoordinateClassifier
    {
        public static readonly string DeviationAttributeName = "outdoor_class_deviation";
        public static readonly string OutdoorClassAttributeName = "outdoor_class";

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
                feature.Attributes.Add(OutdoorClassAttributeName, classification.Classification);
                feature.Attributes.Add(DeviationAttributeName, classification.Deviation);
            }
        }

        public abstract FeatureCollection GetClassification(FeatureCollection input);

        public abstract CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input);

    }
}
