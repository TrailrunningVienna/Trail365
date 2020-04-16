using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public class TrackSegement
    {
        public TrackSegement(CoordinateClassifier classifier)
        {
            this.Classifier = classifier;
        }
        public bool HasCalculatedValue { get; private set; }
        private readonly CoordinateClassifier Classifier;

        public Task<TrackSegement> Worker { get; set; }
        public TrackSegement Previous { get; set; }
        public TrackSegement Next { get; set; }

        public LineString Line { get; set; }

        public CoordinateClassification ResultClassification { get; set; }

        private IFeature prepare = null;
        public void PrepareInterpolation(IFeature f)
        {
            this.HasCalculatedValue = false;
            prepare = f;
        }
        public void StartInterpolation(FeatureCollection facts)
        {
            Guard.AssertNotNull(prepare);
            Guard.AssertNotNull(this.Previous);
            Guard.AssertNotNull(this.Next);
            Guard.AssertNotNull(this.Previous.Worker);
            Guard.AssertNotNull(this.Next.Worker);
            Guard.AssertNotNull(this.Previous.HasCalculatedValue);
            Guard.AssertNotNull(this.Next.HasCalculatedValue);

            this.Worker = Task.Factory.StartNew<TrackSegement>(() =>
            {
                Task.WaitAll(this.Previous.Worker, this.Next.Worker);

                if (this.Previous.ResultClassification.Classification == this.Next.ResultClassification.Classification)
                {
                    this.ResultClassification = new CoordinateClassification(prepare.Geometry, this.Previous.ResultClassification.Classification, this.Previous.ResultClassification.Quality);
                    CoordinateClassifier.ApplyAttribute(prepare, this.ResultClassification);

                }
                else
                {
                    this.ResultClassification = this.Classifier.CreateClassification(facts, this.Line);
                    CoordinateClassifier.ApplyAttribute(prepare, this.ResultClassification);
                }
                return this;
            });
        }

        public void StartCalculation(IFeature f, FeatureCollection facts)
        {
            this.HasCalculatedValue = true;
            this.Worker = Task.Factory.StartNew<TrackSegement>(() =>
            {
                this.ResultClassification = this.Classifier.CreateClassification(facts, this.Line);
                CoordinateClassifier.ApplyAttribute(f, this.ResultClassification);
                return this;
            });
        }

    }
}
