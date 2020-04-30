using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Trail365
{

    public class ClassificationProposal
    {

        /// <summary>
        /// Map geometry/feature where we are looking for!
        /// </summary>
        public readonly Geometry LookupKey;

        /// <summary>
        /// best LineSegment on LookupKey for the current search!
        /// </summary>
        
        public List<LineSegmentProposal> LinkedLineSegments { get; set; }

        public ClassificationProposal(Geometry geometry)
        {
            this.LookupKey = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public double? GetDeviationOrDefault(string classification)
        {
            var res = this.LinkedLineSegments.Where(ll => ll.Classification == classification).OrderBy(ll => ll.ReferenceDistance).FirstOrDefault();
            if (res != null)
            {
                return res.ReferenceDistance;
            }
            return null;
        }

    }
}
