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
    
        public List<LineSegmentProposal> Proposals { get; private set; }

        public ClassificationProposal(Geometry geometry, List<LineSegmentProposal> proposals)
        {
            this.LookupKey = geometry ?? throw new ArgumentNullException(nameof(geometry));
            this.Proposals = proposals ?? throw new ArgumentNullException(nameof(proposals));
        }

        public double? GetDeviationOrDefault(string classification)
        {
            var res = GetBestProposalOrDefault(classification);
            if (res == null) return null;
            return res.ReferenceDistance;
        }

        public LineSegmentProposal GetBestProposalOrDefault(string classification)
        {
            var proposedList = this.Proposals.Where(ll => ll.Classification == classification).ToList();

            if (proposedList.Count == 0)
            {
                return null;
            }

            if (proposedList.Count == 1)
            {
                return proposedList[0];
            }
            //filter by (available factors: distance and angle)
            var res = this.Proposals.Where(ll => ll.Classification == classification).OrderBy(ll => ll.ReferenceDistance).FirstOrDefault();
            return res;
        }
    }
}
