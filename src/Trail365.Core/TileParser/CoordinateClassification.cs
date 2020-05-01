using System;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Trail365
{
    public class CoordinateClassification
    {
        public static string ToDeviation(double? value, string defaultValue = "N/A")
        {
            if (!value.HasValue)
            {
                return $"{defaultValue}";
            }
            else
            {
                double d = (value.Value * Convert.ToDouble(NTSExtensions.DeviationFactor));
                double rounded = Math.Round(d, 3);
                return d.ToString("0.###");
            }
        }

        private static readonly double SoloLimit = Convert.ToDouble(50) / Convert.ToDouble(NTSExtensions.DeviationFactor);
        private static readonly double Tolerance = SoloLimit * 4;
        public static CoordinateClassification CreateFromProposal(ClassificationProposal proposal, Geometry input, ILogger logger)
        {
            string description = null;


            if (proposal == null || proposal.LinkedLineSegments.Count == 0)
            {
                description = $"no {nameof(proposal.LinkedLineSegments)}";
                return new CoordinateClassification(input, CoordinateClassification.Unknown, "N/A", description);
            }

            double? trailProposal = proposal.GetDeviationOrDefault(CoordinateClassification.Trail);
            double? pavedRoadProposal = proposal.GetDeviationOrDefault(CoordinateClassification.PavedRoad);
            double? asphaltedRoadProposal = proposal.GetDeviationOrDefault(CoordinateClassification.AsphaltedRoad);

            description = $"TrailDeviation={ToDeviation(trailProposal)}, PavedDeviation={ToDeviation(pavedRoadProposal)}, AsphaltedDeviation={ToDeviation(asphaltedRoadProposal)}";

            if ((!trailProposal.HasValue) && (!pavedRoadProposal.HasValue) && (!asphaltedRoadProposal.HasValue))
            {
                throw new InvalidOperationException("oops");
            }

            if (trailProposal.HasValue && trailProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposal), description);
            if (pavedRoadProposal.HasValue && pavedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposal), description);
            if (asphaltedRoadProposal.HasValue && asphaltedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposal), description);

            if ((asphaltedRoadProposal.HasValue) && (pavedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                double othersMin = Math.Min(asphaltedRoadProposal.Value, pavedRoadProposal.Value);

                if (trailProposal.Value < othersMin)
                {
                    return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposal), description);
                }

                if (trailProposal.Value+Tolerance < othersMin)
                {
                    logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail preferred over others for acceptable tolerance");
                    return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposal), description);
                }

                if (asphaltedRoadProposal.Value < pavedRoadProposal.Value)
                {
                    //special case: if paved road deviation is higher then asphalt but only with a small factor, then assume paved road usage
                    if (asphaltedRoadProposal + (asphaltedRoadProposal.Value+Tolerance) > pavedRoadProposal)
                    {
                        logger.LogTrace($"[{nameof(CreateFromProposal)}]: paved preferred over asphalted for acceptable tolerance");
                        return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposal), description);
                    }
                    return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposal), description);
                }
                else
                {
                    return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposal), description);
                }
                throw new InvalidOperationException("decision required");
            }

            if ((pavedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                if (trailProposal.Value < pavedRoadProposal.Value + Tolerance)
                {
                    logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail preferred over paved for acceptable tolerance");
                    return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposal), description);
                }
                return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposal), description);
            }


            if ((asphaltedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                if (trailProposal.Value < asphaltedRoadProposal + Tolerance)
                {
                    logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail preferred over asphalted for acceptable tolerance");
                    return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposal), description);
                }
                return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposal), description);
            }

            //last exit: without limit
            if (trailProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposal), description);
            if (pavedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposal), description);
            if (asphaltedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposal), description);

            throw new InvalidOperationException("we should never come here");
        }

        public static readonly string Unknown = "unknown";
        public static readonly string Trail = "trail";
        public static readonly string PavedRoad = "road";
        public static readonly string AsphaltedRoad = "motorway";

        public CoordinateClassification(Geometry location, string classification, string deviation, string description)
        {
            this.Location = location ?? throw new ArgumentNullException(nameof(location));
            this.Classification = classification ?? throw new ArgumentNullException(nameof(classification));
            this.Deviation = deviation ?? throw new ArgumentNullException(nameof(deviation));
            this.Description = description;
        }

        public Geometry Location { get; private set; }
        public string Classification { get; private set; }
        public string Deviation { get; private set; }

        /// <summary>
        /// written as property to the featurecollection/geoJson
        /// </summary>
        public string Description { get; set; }
    }
}
