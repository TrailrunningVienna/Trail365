using System;

namespace Trail365.DTOs
{
    public class PlaceDto
    {
        public string GetExternalID()
        {
            return this.Name.ToLowerInvariant().Replace(" ", string.Empty).Replace(";", string.Empty).Replace(",", string.Empty).Replace("-", string.Empty).Trim();
        }

        public Guid ID { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Zip { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        /// <summary>
        /// TwoLetterISORegionName
        /// The two-letter code defined in ISO 3166 for the country/region
        /// </summary>
        public string CountryTwoLetterISOCode { get; set; }
    }
}
