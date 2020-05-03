using NetTopologySuite.Algorithm;

namespace Trail365
{
    public class TrackAnalyzerSettings
    {
        public double TerminateDistance { get; set; } = NTSExtensions.DeviationToDistance(10000);
        public double MaximumAngleDiff { get; set; } = AngleUtility.PiOver4;

    }
}
