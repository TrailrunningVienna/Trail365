using System;
using NetTopologySuite.Algorithm;
namespace Trail365
{
    public class TrackAnalyzerSettings
    {

        public TrackAnalyzerSettings() : this(false)
        {
        }
        public TrackAnalyzerSettings(bool isStronger)
        {
            IsStronger = isStronger;
        }

        private readonly bool IsStronger = false;
        public double TerminateDistance { get; set; } = NTSExtensions.DeviationToDistance(10000);
        public double MaximumAngleDiff { get; set; } = AngleUtility.PiOver4;

        public TrackAnalyzerSettings ToStronger()
        {
            if (this.IsStronger) throw new InvalidOperationException("Cannot create stronger settings from strong");
            return new TrackAnalyzerSettings(true)
            {
                TerminateDistance = this.TerminateDistance / 2,
                MaximumAngleDiff = this.MaximumAngleDiff / 2,
            };
        }
    }
}
