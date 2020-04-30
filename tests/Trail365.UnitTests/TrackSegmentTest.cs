using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.UnitTests.Builders;
using Xunit;
using System.Linq;

namespace Trail365.UnitTests
{


    [Trait("Category", "BuildVerification")]
    public class TrackSegmentTest
    {


       

        [Fact]
        public void ShouldReturnProposals()
        {
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();

            TrackSegement ts = new TrackSegement(facts, 1); 

            ts.ClassificationGetter = (f) =>
            {
                Assert.Equal(2, f.Geometry.Coordinates.Length);
                return f.GetHashCode().ToString();
            };

            var result = ts.GetClassificationProposals(testLine).ToList();

            //Assert.Equal(LineCoordinates.Positive45DegreeLineFrom10.Length-1, result.Count);
        }



        [Fact]
        public void ShouldNotReturnClassificationForDistance()
        {
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom10).Build();

            TrackSegement ts = new TrackSegement(facts, 1); //testline 10 units away, TerminateDistance 1 => No classification possible!

            ts.ClassificationGetter = (f) =>
            {
                Assert.Equal(2, f.Geometry.Coordinates.Length);
                return f.GetHashCode().ToString();
            };

            var result = ts.GetClassificationProposals(testLine).ToList();

            //Assert.Equal(LineCoordinates.Positive45DegreeLineFrom10.Length-1, result.Count);
        }


        [Fact]
        public void ShouldReturnClassificationForDistance()
        {
            FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
            LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();

            TrackSegement ts = new TrackSegement(facts, 0.1);

            ////ts.GetProposal

            //ts.ClassificationGetter = (f) => f.GetHashCode().ToString();

            //ts.ClassificationFactory = (prop, sl) => new CoordinateClassification(prop.LookupKey, CoordinateClassification.Unknown, "1", "ole");

            //var result = ts.GetClassification(testLine).ToList();
            //Assert.Equal(LineCoordinates.Positive45DegreeLineFrom0.Length - 1, result.Count);

            //result.ForEach(r => Assert.Equal(CoordinateClassification.Unknown, r.Classification));
        }







        //[Fact]
        //public void Identical()
        //{
        //    FeatureCollection facts = FeatureCollectionBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
        //    LineString testLine = LineStringBuilder.CreateLineString(LineCoordinates.Positive45DegreeLineFrom0).Build();
        //    TrackSegement ts = new TrackSegement(facts,10);
        //    var result = ts.GetClassification(testLine);
        //}



    }
}
