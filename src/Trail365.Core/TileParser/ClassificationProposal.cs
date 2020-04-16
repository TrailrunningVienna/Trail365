using NetTopologySuite.Geometries;

namespace Trail365
{
    public class ClassificationProposal
    {
        public Geometry LookupKey { get; set; }

        public double DistanceToNearest { get; set; }
        public string Classification { get; set; }

        internal static readonly double ca05MVertical = 0.00005;
        internal static readonly double ca10MVertical = ca05MVertical * 2;
        internal static readonly double ca15MVertical = ca05MVertical * 3;
        internal static readonly double ca20MVertical = ca05MVertical * 4;
        internal static readonly double ca25MVertical = ca05MVertical * 5;
        internal static readonly double ca30MVertical = ca05MVertical * 6;
        internal static readonly double ca35MVertical = ca05MVertical * 7;
        internal static readonly double ca40MVertical = ca05MVertical * 8;
        internal static readonly double ca45MVertical = ca05MVertical * 9;
        internal static readonly double ca50MVertical = ca05MVertical * 10;
        internal static readonly double ca55MVertical = ca05MVertical * 11;
        internal static readonly double ca60MVertical = ca05MVertical * 12;
        internal static readonly double ca65MVertical = ca05MVertical * 13;
        internal static readonly double ca70MVertical = ca05MVertical * 14;
        internal static readonly double ca75MVertical = ca05MVertical * 15;
        internal static readonly double ca80MVertical = ca05MVertical * 16;
        internal static readonly double ca85MVertical = ca05MVertical * 17;
        internal static readonly double ca90MVertical = ca05MVertical * 18;
        internal static readonly double ca95MVertical = ca05MVertical * 19;

        /// <summary>
        /// 0...ignore more than tollerance
        /// >=1... better and better!
        /// </summary>
        /// <returns></returns>
        public int GetQuality()
        {
            int quality = 0; //ignore the result!
            if (this.DistanceToNearest < ca95MVertical) quality = 1;
            if (this.DistanceToNearest < ca90MVertical) quality = 2;
            if (this.DistanceToNearest < ca85MVertical) quality = 3;
            if (this.DistanceToNearest < ca80MVertical) quality = 4;
            if (this.DistanceToNearest < ca75MVertical) quality = 5;
            if (this.DistanceToNearest < ca70MVertical) quality = 6;
            if (this.DistanceToNearest < ca65MVertical) quality = 7;
            if (this.DistanceToNearest < ca60MVertical) quality = 8;
            if (this.DistanceToNearest < ca55MVertical) quality = 9;
            if (this.DistanceToNearest < ca50MVertical) quality = 10;
            if (this.DistanceToNearest < ca45MVertical) quality = 11;
            if (this.DistanceToNearest < ca40MVertical) quality = 12;
            if (this.DistanceToNearest < ca35MVertical) quality = 13;
            if (this.DistanceToNearest < ca30MVertical) quality = 14;
            if (this.DistanceToNearest < ca25MVertical) quality = 15;
            if (this.DistanceToNearest < ca20MVertical) quality = 16;
            if (this.DistanceToNearest < ca15MVertical) quality = 17;
            if (this.DistanceToNearest < ca10MVertical) quality = 18;
            if (this.DistanceToNearest < ca05MVertical) quality = 19;
            System.Diagnostics.Debug.WriteLine($"Dist={this.DistanceToNearest.ToString("0.0000000000")}, Quality={quality}");
            return quality;
        }
    }
}
