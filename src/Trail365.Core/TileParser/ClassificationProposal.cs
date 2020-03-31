using NetTopologySuite.Geometries;

namespace Trail365
{
    public class ClassificationProposal
    {
        public Geometry LookupKey { get; set; }

        public double DistanceToNearest { get; set; }
        public string Classification { get; set; }


        internal static readonly double ca5MVertical = ca10MVertical / 2;
        internal static readonly double ca10MVertical = 0.00008; //cartesian coordinate system estimate
        internal static readonly double ca15MVertical = ca5MVertical * 3;
        internal static readonly double ca20MVertical = ca10MVertical * 2;
        internal static readonly double ca25MVertical = ca5MVertical * 5;

        /// <summary>
        /// 0...ignore more than tollerance
        /// >=1... better and better!
        /// </summary>
        /// <returns></returns>
        public int GetQuality()
        {
            int quality = 0; //ignore the result!
            if (this.DistanceToNearest < ca25MVertical) quality = 1;
            if (this.DistanceToNearest < ca20MVertical) quality = 2;
            if (this.DistanceToNearest < ca15MVertical) quality = 3;
            if (this.DistanceToNearest < ca10MVertical) quality = 4;
            if (this.DistanceToNearest < ca5MVertical) quality = 5;
            return quality;
        }
    }
}
