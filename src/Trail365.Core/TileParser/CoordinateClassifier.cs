using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{

    public abstract class CoordinateClassifier
    {
        public static readonly string DeviationAttributeName = "outdoor_class_deviation";
        public static readonly string OutdoorClassAttributeName = "outdoor_class";
        public static readonly string DescriptionAttributeName = "outdoor_class_description";

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

            if (!string.IsNullOrEmpty(classification.Description))
            {
                if (feature.Attributes == null)
                {
                    feature.Attributes = new AttributesTable();
                }
                feature.Attributes.Add(DescriptionAttributeName, classification.Description);
            }

        }

        protected ILogger Logger { get; private set; } = NullLogger.Instance;

        public void AssignLogger(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            this.Logger = logger;
        }

        public abstract FeatureCollection GetClassification(FeatureCollection input);

        //public abstract CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input);

    }
}
