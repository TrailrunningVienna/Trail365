using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace Trail365
{
    /// <summary>
    /// </summary>
    public class LineSegmentProposal
    {
        public LineSegmentProposal(Geometry owner)
        {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        /// <summary>
        /// Angle between the two coordinates that are building THIS Segment!
        /// </summary>
        public double NormalizedAngle { get; set; }

        public LineSegment Segement { get; set; }

        public double? ReferenceDistance { get; set; }

        public LineSegmentProposal Reference { get; set; }

        /// <summary>
        /// Segment (coordinates) must be reside on the owner geometry 
        /// </summary>
        public Geometry Owner { get; private set; }

        public string Classification { get; set; }

        public double GetAngleDiffToReference()
        {
            return GetAngleDiff(this.NormalizedAngle, this.Reference.NormalizedAngle);
        }

        public static double GetAngleDiff(double normalizedAngle1, double normalizedAngle2)
        {
            return AngleUtility.Diff(normalizedAngle1, normalizedAngle2);

        }

    }
}
