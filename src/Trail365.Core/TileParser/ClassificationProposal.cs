using NetTopologySuite.Geometries;

namespace Trail365
{
    public class ClassificationProposal
    {
        public Geometry LookupKey { get; set; }

        public double DistanceToNearest { get; set; }
        public string Classification { get; set; }


        internal static readonly double ca10MVertical = 0.00008569132718; //cartesian coordinate system estimate
        internal static readonly double ca20MVertical = ca10MVertical * 2;

        /// <summary>
        /// 3... best
        /// 0...lowest
        /// </summary>
        /// <returns></returns>
        public int GetQuality()
        {
            double ca5MVertical = ca10MVertical / 2;

            int quality = 1; //10-20m

            if (this.DistanceToNearest < ca5MVertical)
            {
                quality = 3; //better then 5m
            }
            else
            {
                if (this.DistanceToNearest < ca10MVertical)
                {
                    quality = 2; //better then 5 meter, not better then 10
                }
            }

            if (this.DistanceToNearest>ca20MVertical)
            {
                quality = 0;
            }

            return quality;
        }
    }
}
