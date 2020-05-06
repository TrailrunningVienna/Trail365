using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace Trail365
{
    public class CoordinateClassification
    {
        public static string ToAngleDiff(double? value, string defaultValue = "N/A")
        {
            if (!value.HasValue)
            {
                return $"{defaultValue}";
            }
            else
            {
                double d = AngleUtility.ToDegrees(value.Value);
                int rounded = Convert.ToInt32(Math.Round(d, 0));
                return rounded.ToString();
            }
        }

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
                return rounded.ToString("0.###");
            }
        }

        private static readonly double SoloLimit = Convert.ToDouble(50) / Convert.ToDouble(NTSExtensions.DeviationFactor);

        private static readonly double DistanceTolerance = SoloLimit * 4;

        private static readonly double AngleTolerance = AngleUtility.PiOver4 / 4;

        private static bool IsIncluded(TrackAnalyzerSettings settings, LineSegmentProposal proposal)
        {

            double angleDiff = proposal.GetAngleDiffToReference(); //calculate it only one time, we need the value more often!

            if ((proposal.ReferenceDistance.Value <= settings.TerminateDistance) && (angleDiff <= settings.MaximumAngleDiff))
            {
                return true;
            }

            if ((proposal.ReferenceDistance.Value > settings.TerminateDistance) && (angleDiff > settings.MaximumAngleDiff))
            {
                return false;
            }


            //one of them
            if ((proposal.ReferenceDistance.Value <= settings.TerminateDistance))
            {
                double angleDelta = angleDiff - settings.MaximumAngleDiff;
                if (angleDelta < (AngleUtility.PiOver4 / 4))
                {
                    return true;
                }
            }
            else
            {
                Guard.Assert(angleDiff <= settings.MaximumAngleDiff);
                //if angle is really perfect then allow lot more distance
                double distToleranceFactor = 1.1;

                if (angleDiff < (settings.MaximumAngleDiff / 8)) //default for maximum is 45
                {
                    distToleranceFactor = 2.0;
                }

                //distance to big, means angle is OK
                if (proposal.ReferenceDistance.Value < (settings.TerminateDistance * distToleranceFactor))
                {
                    return true;
                }

            }

            return false;

        }
        public static CoordinateClassification CreateFromProposal(ClassificationProposal proposal, Geometry input, TrackAnalyzerSettings settings, ILogger logger)
        {
            string description = null;


            if (proposal == null || proposal.Proposals.Count == 0)
            {
                description = $"no {nameof(proposal.Proposals)}";
                return new CoordinateClassification(input, CoordinateClassification.Unknown, "N/A", description);
            }


            var proposalForTrail = proposal.GetBestProposalOrDefault(CoordinateClassification.Trail);
            var proposalForPavedRoad = proposal.GetBestProposalOrDefault(CoordinateClassification.PavedRoad);
            var proposalForAsphlatedRoad = proposal.GetBestProposalOrDefault(CoordinateClassification.AsphaltedRoad);

            List<string> excludedDescriptions = new List<string>();

            if ((proposalForTrail != null) && !IsIncluded(settings, proposalForTrail))
            {
                excludedDescriptions.Add($"ExcludedTrailDeviation={ToDeviation(proposalForTrail.ReferenceDistance)}, ExcludedTrailAngleDiff={ToAngleDiff(proposalForTrail.GetAngleDiffToReference())}");
                proposalForTrail = null;
            }

            if ((proposalForPavedRoad != null) && !IsIncluded(settings, proposalForPavedRoad))
            {
                excludedDescriptions.Add($"ExcludedPavedDeviation={ToDeviation(proposalForPavedRoad.ReferenceDistance)}, ExcludedPavedAngleDiff={ToAngleDiff(proposalForPavedRoad.GetAngleDiffToReference())}");
                proposalForPavedRoad = null;
            }

            if ((proposalForAsphlatedRoad != null) && !IsIncluded(settings, proposalForAsphlatedRoad))
            {
                excludedDescriptions.Add($"ExcludedAsphaltedDeviation={ToDeviation(proposalForAsphlatedRoad.ReferenceDistance)}, ExcludedAsphaltedAngleDiff={ToAngleDiff(proposalForAsphlatedRoad.GetAngleDiffToReference())}");
                proposalForAsphlatedRoad = null;
            }

            double? trailProposalDistance = proposalForTrail?.ReferenceDistance;
            double? pavedRoadProposalDistance = proposalForPavedRoad?.ReferenceDistance;
            double? asphaltedRoadProposalDistance = proposalForAsphlatedRoad?.ReferenceDistance;

            Guard.Assert(trailProposalDistance.HasValue == (proposalForTrail != null));
            Guard.Assert(pavedRoadProposalDistance.HasValue == (proposalForPavedRoad != null));
            Guard.Assert(asphaltedRoadProposalDistance.HasValue == (proposalForAsphlatedRoad != null));

            double? trailProposalAngleDiff = proposalForTrail?.GetAngleDiffToReference();
            double? pavedRoadProposalAngleDiff = proposalForPavedRoad?.GetAngleDiffToReference();
            double? asphaltedRoadProposalAngleDiff = proposalForAsphlatedRoad?.GetAngleDiffToReference();

            List<string> descriptionItems = new List<string>();

            if (proposalForTrail != null)
            {
                descriptionItems.Add($"TrailDeviation={ToDeviation(trailProposalDistance)}, TrailAngleDiff={ToAngleDiff(trailProposalAngleDiff)}");
            }

            if (proposalForPavedRoad != null)
            {
                descriptionItems.Add($"PavedDeviation={ToDeviation(pavedRoadProposalDistance)}, PavedAngleDiff={ToAngleDiff(pavedRoadProposalAngleDiff)}");
            }

            if (proposalForAsphlatedRoad != null)
            {
                descriptionItems.Add($"AsphaltedDeviation={ToDeviation(asphaltedRoadProposalDistance)}, AsphaltetAngleDiff={ToAngleDiff(asphaltedRoadProposalAngleDiff)}");
            }

            description = string.Join(", ", descriptionItems.Concat(excludedDescriptions));

            if ((!trailProposalDistance.HasValue) && (!pavedRoadProposalDistance.HasValue) && (!asphaltedRoadProposalDistance.HasValue))
            {
                return new CoordinateClassification(input, CoordinateClassification.Unknown, "N/A", description);
            }

            if (trailProposalDistance.HasValue && trailProposalDistance.Value < SoloLimit)
            {
                return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposalDistance), description);
            }

            if (pavedRoadProposalDistance.HasValue && pavedRoadProposalDistance.Value < SoloLimit)
            {
                return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposalDistance), description);
            }

            if (asphaltedRoadProposalDistance.HasValue && asphaltedRoadProposalDistance.Value < SoloLimit)
            {
                return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposalDistance), description);
            }

            if ((asphaltedRoadProposalDistance.HasValue) && (pavedRoadProposalDistance.HasValue) && trailProposalDistance.HasValue)
            {
                double othersDistanceMin = Math.Min(asphaltedRoadProposalDistance.Value, pavedRoadProposalDistance.Value);
                double othersAngleDiffMin = Math.Min(asphaltedRoadProposalAngleDiff.Value, pavedRoadProposalAngleDiff.Value);

                if ((trailProposalDistance.Value < othersDistanceMin) || (trailProposalAngleDiff.Value < othersAngleDiffMin))
                {
                    logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail better in distance or angle");
                    return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposalDistance), description);
                }

                if ((trailProposalDistance.Value < (othersDistanceMin + DistanceTolerance)) || (trailProposalAngleDiff.Value < (othersAngleDiffMin + AngleTolerance)))
                {
                    logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail preferred over others for acceptable tolerance");
                    return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposalDistance), description);
                }

                if (asphaltedRoadProposalDistance.Value < pavedRoadProposalDistance.Value)
                {
                    //special case: if paved road deviation is higher then asphalt but only with a small factor, then assume paved road usage
                    if ((asphaltedRoadProposalDistance.Value + DistanceTolerance) > pavedRoadProposalDistance)
                    {
                        logger.LogTrace($"[{nameof(CreateFromProposal)}]: paved preferred over asphalted for acceptable tolerance");
                        return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposalDistance), description);
                    }
                    return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposalDistance), description);
                }
                else
                {
                    return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposalDistance), description);
                }
                throw new InvalidOperationException("decision required");
            }

            if ((pavedRoadProposalDistance.HasValue) && trailProposalDistance.HasValue)
            {
                if (trailProposalDistance.Value < pavedRoadProposalDistance.Value + DistanceTolerance)
                {
                    if (trailProposalAngleDiff < pavedRoadProposalAngleDiff + AngleTolerance)
                    {
                        logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail preferred over paved for acceptable distance tolerance AND angle tolerance");
                        return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposalDistance), description);
                    }
                    else
                    {
                        logger.LogTrace($"[{nameof(CreateFromProposal)}]: paved preferred over trail for acceptable distance and angle");
                        return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposalDistance), description);

                    }
                }
                return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposalDistance), description);
            }


            if ((asphaltedRoadProposalDistance.HasValue) && trailProposalDistance.HasValue)
            {
                //usecase trail & asphalt with good distance (crossing road)
                //check angle, best case it is a big difference
                if (trailProposalDistance.Value < asphaltedRoadProposalDistance + DistanceTolerance)
                {
                    if (trailProposalAngleDiff < asphaltedRoadProposalAngleDiff + AngleTolerance)
                    {
                        logger.LogTrace($"[{nameof(CreateFromProposal)}]: trail preferred over asphalted for acceptable distance tolerance AND angle tolerance");
                        return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposalDistance), description);
                    }
                    else
                    {
                        logger.LogTrace($"[{nameof(CreateFromProposal)}]: asphalt preferred over trail for better angle (ignoring better distance for trail)");
                        return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposalDistance), description);
                    }
                }
                logger.LogTrace($"[{nameof(CreateFromProposal)}]: asphalt preferred over trail for better distance and angle");
                return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposalDistance), description);
            }

            //last exit: without limit
            if (trailProposalDistance.HasValue) return new CoordinateClassification(input, CoordinateClassification.Trail, ToDeviation(trailProposalDistance), description);
            if (pavedRoadProposalDistance.HasValue) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, ToDeviation(pavedRoadProposalDistance), description);
            if (asphaltedRoadProposalDistance.HasValue) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, ToDeviation(asphaltedRoadProposalDistance), description);

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
