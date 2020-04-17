using NetTopologySuite.Geometries;
using System;

namespace Trail365
{
    public class ClassificationProposal
    {
        public Geometry LookupKey { get; set; }

        public double DistanceToNearest { get; set; }
        public string Classification { get; set; }

        /// <summary>
        /// integer about deviation - NO UNIT!
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetDeviation()
        {
            double derived = this.DistanceToNearest * 10000000;
            int quality = Convert.ToInt32(Math.Round(derived));
            System.Diagnostics.Debug.WriteLine($"Dist={this.DistanceToNearest.ToString("0.0000000000")}, Quality={quality}");
            return quality;
        }
    }
}
