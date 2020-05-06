using System;
using NetTopologySuite.Algorithm;

namespace Trail365
{
    public class TrackAnalyzerSettings
    {

        public TrackAnalyzerSettings() : this(false, false)
        {
        }

        public TrackAnalyzerSettings(bool isStronger, bool isWeaker)
        {
            IsStronger = isStronger;
            IsWeaker = isWeaker;
        }

        private readonly bool IsStronger = false;
        private readonly bool IsWeaker = false;

        /// <summary>
        /// Defaultvalue Deviation 10.000
        /// </summary>
        public double TerminateDistance { get; set; } = NTSExtensions.DeviationToDistance(10000);

        /// <summary>
        /// default is PiOver4 (means 180/4)= 45;
        /// </summary>
        public double MaximumAngleDiff { get; set; } = AngleUtility.PiOver4;

        public TrackAnalyzerSettings ToStronger()
        {
            if (this.IsStronger) throw new InvalidOperationException("Cannot create stronger settings from strong");

            if (this.IsWeaker)
            {
                return new TrackAnalyzerSettings(true, false)
                {
                    TerminateDistance = this.TerminateDistance / 4,
                    MaximumAngleDiff = this.MaximumAngleDiff / 4,
                };
            }
            else
            {
                return new TrackAnalyzerSettings(true, false)
                {
                    TerminateDistance = this.TerminateDistance / 2,
                    MaximumAngleDiff = this.MaximumAngleDiff / 2,
                };
            }
        }

        public TrackAnalyzerSettings ToWeaker()
        {
            if (this.IsWeaker) throw new InvalidOperationException("Cannot create weaker settings from weak");
            if (this.IsStronger)
            {
                return new TrackAnalyzerSettings(false, true)
                {
                    TerminateDistance = this.TerminateDistance * 4,
                    MaximumAngleDiff = this.MaximumAngleDiff * 4,
                };

            }
            else
            {
                return new TrackAnalyzerSettings(false, true)
                {
                    TerminateDistance = this.TerminateDistance * 2,
                    MaximumAngleDiff = this.MaximumAngleDiff * 2,
                };
            }
        }

    }
}
