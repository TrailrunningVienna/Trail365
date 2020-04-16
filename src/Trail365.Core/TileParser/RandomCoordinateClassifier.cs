using System;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public class RandomCoordinateClassifier : CoordinateClassifier
    {

        public RandomCoordinateClassifier()
        {
            sequenceClass = classes[random.Next(0, 3)];
            sequenceLength = random.Next(3, 5) * random.Next(2, 4);
        }
        private readonly Random random = new Random();
        private static readonly string[] classes = new string[] { CoordinateClassification.Trail, CoordinateClassification.PavedRoad, CoordinateClassification.AsphaltedRoad, CoordinateClassification.Unknown };
        private int sequenceLength;
        private string sequenceClass;

        public override CoordinateClassification CreateClassification(FeatureCollection facts, Geometry input)
        {
            sequenceLength -= 1;
            if (sequenceLength == 0)
            {
                sequenceLength = random.Next(3, 7) * random.Next(3, 7);
                sequenceClass = classes[random.Next(0, classes.Length)];
            }
            return new CoordinateClassification(input, sequenceClass,"random");
        }


        public override FeatureCollection GetClassification(FeatureCollection input)
        {
            //Input MUST be one line (2 points) per Features
            var splitted = input.SplitIntoFeaturePerLineSegment();
            CoordinateClassification lastSegmentClass = null;
            LineString lastSegement = null;
            CoordinateClassification beforeLasSegmentClass = null;
            bool lastSkipped = false;

            for (int i = 0; i < splitted.Count; i++)
            {
                var f = splitted[i];
                LineString currentSegment = (LineString)f.Geometry;

                Guard.AssertNotNull(currentSegment);
                Guard.Assert(currentSegment.Count == 2);

                if (lastSegement == null)
                {
                    Guard.Assert(lastSkipped == false);
                    Guard.AssertNull(lastSegmentClass);
                    lastSegmentClass = this.CreateClassification(null, currentSegment);
                    lastSegement = currentSegment;
                    ApplyAttribute(f, lastSegmentClass);
                    continue;
                }

                if ((lastSkipped == false) && (lastSegement.EndPoint == currentSegment.StartPoint))
                {
                    Guard.Assert(lastSkipped == false);
                    beforeLasSegmentClass = lastSegmentClass;
                    lastSegement = currentSegment;
                    lastSegmentClass = null;
                    lastSkipped = true;
                    continue; //skip this!
                }
                else
                {
                    var currentClass = this.CreateClassification(null, currentSegment);
                    ApplyAttribute(f, currentClass);
                    if (lastSkipped)
                    {
                        Guard.AssertNotNull(beforeLasSegmentClass);
                        if (beforeLasSegmentClass.Classification == currentClass.Classification)
                        {
                            //use the same value like current and beforelast without recalculating!
                            var f1 = splitted[i - 1];
                            lastSegmentClass = new CoordinateClassification(f1.Geometry, currentClass.Classification,currentClass.Quality);
                            ApplyAttribute(f1, lastSegmentClass);
                        }
                        else
                        {
                            lastSegmentClass = this.CreateClassification(null, lastSegement);
                            ApplyAttribute(splitted[i - 1], lastSegmentClass);
                        }
                    }
                    lastSegement = currentSegment;
                    beforeLasSegmentClass = lastSegmentClass;
                    lastSegmentClass = currentClass;
                    lastSkipped = false;
                }
            }

            if (lastSkipped)
            {
                var reClass = this.CreateClassification(null, lastSegement);
                ApplyAttribute(splitted[splitted.Count - 1], reClass);
            }

            return splitted.Merge(includeQuality:true);
        }


    }
}
