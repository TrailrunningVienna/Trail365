using NetTopologySuite.Geometries;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Trail365
{
    public class ClassificationProposal
    {
        public Geometry LookupKey { get; set; }

        /// <summary>
        /// integer about deviation - NO UNIT!
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetDeviation(string classification)
        {
            double distance = this.Classifications[classification];
            int quality = NTSExtensions.GetDeviation(distance);
            System.Diagnostics.Debug.WriteLine($"Classification={classification}, Dist={distance.ToString("0.0000000000")}, Quality={quality}");
            return quality;
        }

        public int? GetDeviationOrDefault(string classification)
        {
            if (this.Classifications.ContainsKey(classification) == false) return null;
            return GetDeviation(classification);
        }

        public Dictionary<string,double> Classifications { get; internal set; }
    }
}
