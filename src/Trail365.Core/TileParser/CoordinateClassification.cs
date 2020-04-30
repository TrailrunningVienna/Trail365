using System;
using NetTopologySuite.Geometries;

namespace Trail365
{
    public class CoordinateClassification
    {
        private static readonly double SoloLimit = Convert.ToDouble(50) / Convert.ToDouble(NTSExtensions.DeviationFactor);

        public static CoordinateClassification CreateFromProposal(ClassificationProposal proposal, Geometry input)
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

            description = $"TrailDeviation={trailProposal}, PavedDeviation={pavedRoadProposal}, AsphaltedDeviation={asphaltedRoadProposal}";

            if ((!trailProposal.HasValue) && (!pavedRoadProposal.HasValue) && (!asphaltedRoadProposal.HasValue))
            {
                throw new InvalidOperationException("oops");
            }

            if (trailProposal.HasValue && trailProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
            if (pavedRoadProposal.HasValue && pavedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
            if (asphaltedRoadProposal.HasValue && asphaltedRoadProposal.Value < SoloLimit) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);

            if ((asphaltedRoadProposal.HasValue) && (pavedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                double othersMin = Math.Min(asphaltedRoadProposal.Value, pavedRoadProposal.Value);

                if (trailProposal.Value < othersMin) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);

                if (asphaltedRoadProposal.Value < pavedRoadProposal.Value)
                {
                    return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);
                }
                else
                {
                    return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
                }
                throw new InvalidOperationException("decision required");
            }

            if ((pavedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                if (trailProposal.Value < pavedRoadProposal + SoloLimit)
                {
                    return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
                }
                return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
            }


            if ((asphaltedRoadProposal.HasValue) && trailProposal.HasValue)
            {
                if (trailProposal.Value < asphaltedRoadProposal + SoloLimit)
                {
                    return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
                }
                return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);
            }

            //last exit: without limit
            if (trailProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.Trail, trailProposal.Value.ToString(), description);
            if (pavedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.PavedRoad, pavedRoadProposal.Value.ToString(), description);
            if (asphaltedRoadProposal.HasValue) return new CoordinateClassification(input, CoordinateClassification.AsphaltedRoad, asphaltedRoadProposal.Value.ToString(), description);

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
