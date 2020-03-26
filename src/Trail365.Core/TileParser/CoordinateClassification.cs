using System;
using NetTopologySuite.Geometries;

namespace Trail365
{
    public class CoordinateClassification
    {
        public CoordinateClassification(Geometry location, string classification)
        {
            this.Location = location ?? throw new ArgumentNullException(nameof(location));
            this.Classification = classification ?? throw new ArgumentNullException(nameof(classification));
        }
        public Geometry Location { get; private set; }
        public string Classification { get; private set; }
    }
}
