using System;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Trail365
{
    public class NullCoordinateClassifier : CoordinateClassifier
    {
        //public override CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input)
        //{
        //    throw new NotImplementedException();
        //}

        public override FeatureCollection GetClassification(FeatureCollection input)
        {
            throw new NotImplementedException();
        }
    }
}
