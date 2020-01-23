using System;
using Trail365.Entities;

namespace Trail365.DTOs
{
    public class TrailDto
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string InternalDescription { get; set; }
        public string Excerpt { get; set; }
        public byte[] Gpx { get; set; }
        public AccessLevel GpxDownloadAccess { get; set; }
        public AccessLevel ListAccess { get; set; }
        public DateTime? ScrapedUtc { get; set; }
        public Guid? GpxID { get; set; }

        public int? DistanceMeters { get; set; }

        public int? AscentMeters { get; set; }

        public int? DescentMeters { get; set; }

        public int? MinimumAltitude { get; set; }

        public int? MaximumAltitude { get; set; }
    }
}
