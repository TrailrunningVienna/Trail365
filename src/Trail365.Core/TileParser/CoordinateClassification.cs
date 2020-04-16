using System;
using NetTopologySuite.Geometries;

namespace Trail365
{
    public class CoordinateClassification
    {
        public static readonly string Unknown = "unknown";
        public static readonly string Trail = "trail";
        public static readonly string PavedRoad = "road";
        public static readonly string AsphaltedRoad = "motorway";

        public CoordinateClassification(Geometry location, string classification, string quality)
        {
            this.Location = location ?? throw new ArgumentNullException(nameof(location));
            this.Classification = classification ?? throw new ArgumentNullException(nameof(classification));
            this.Quality = quality ?? throw new ArgumentNullException(nameof(quality));
        }

        public Geometry Location { get; private set; }
        public string Classification { get; private set; }

        public string Quality { get; private set; }
    }
}
