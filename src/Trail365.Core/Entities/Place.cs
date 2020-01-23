using System;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    /// <summary>
    /// onl Key and Name are mandatory, everything else can be null!
    /// </summary>
    public class Place
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Zip { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        /// <summary>
        /// precision of Longitude/Latitude, higher value for radius means lower precision
        /// null means not defined!
        /// </summary>
        public int? Radius { get; set; }

        /// <summary>
        /// TwoLetterISORegionName
        /// </summary>
        public string CountryTwoLetterISOCode { get; set; }

        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        public string ExternalID { get; set; }

        public string ExternalSource { get; set; }

        public string MeetingPoint { get; set; }

        public bool IsCityPartOfTheName { get; set; } = false;
    }
}
