using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Trail365.Internal;

namespace Trail365.Entities
{
    public class Trail
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string CreateValidFileName()
        {
            //string baseLine = $"{this.Name}-{this.ID.ToString("N")}".Replace(" ", string.Empty);
            string baseLine = $"{this.ID.ToString("N")}".ToLowerInvariant(); //special characters in name are causing a lot of problems inside AzureBlob originalFileName!
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                baseLine.Replace(c.ToString(), string.Empty);
            }
            string result = baseLine.Substring(0, 32);
            Guard.Assert(result.Contains(".gpx.gpx") == false);
            return result;
        }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Description only available on the Backend!
        /// </summary>
        public string InternalDescription { get; set; }

        public string Excerpt { get; set; }

        public Guid? GpxBlobID { get; set; }

        public Blob GpxBlob { get; set; }


        public Guid? AnalyzerBlobID { get; set; }

        public Blob AnalyzerBlob { get; set; }

        /// <summary>
        /// userID of the owner, null if "system" or "undefined"
        /// </summary>
        public Guid? OwnerID { get; set; }

        public string ExternalID { get; set; }

        public string ExternalSource { get; set; }

        public Guid? PreviewImageID { get; set; }

        public Blob PreviewImage { get; set; }

        /// <summary>
        /// small means 240xYYY (1.5 or 1.49
        /// </summary>
        public Guid? SmallPreviewImageID { get; set; } //http://tim-stanley.com/post/standard-web-digital-image-sizes/

        public Blob SmallPreviewImage { get; set; }

        /// <summary>
        /// Medium means 460xYYY (1.5 or 1.49
        /// </summary>
        public Guid? MediumPreviewImageID { get; set; } //http://tim-stanley.com/post/standard-web-digital-image-sizes/

        public Blob MediumPreviewImage { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        /// <summary>
        /// map-images scraped
        /// </summary>
        public DateTime? ScrapedUtc { get; set; }

        /// <summary>
        /// means Download-Access (Permission level)
        /// </summary>
        [Column("GpxDownloadAccess")]
        public AccessLevel GpxDownloadAccess { get; set; } = AccessLevel.User;

        public AccessLevel ListAccess { get; set; } = AccessLevel.User;

        //REGION STATISTIC DATA

        /// <summary>
        /// in meters
        /// </summary>
        public int? DistanceMeters { get; set; }

        public int? AscentMeters { get; set; }

        public int? DescentMeters { get; set; }

        public int? MinimumAltitude { get; set; }

        public int? MaximumAltitude { get; set; }

        public int? AltitudeAtStart { get; set; }

        /// <summary>
        /// pure trail
        /// </summary>
        public int? UnpavedTrailMeters { get; set; }

        /// <summary>
        /// typically forestry road
        /// </summary>
        public int? PavedRoadMeters { get; set; }

        public int? AsphaltedRoadMeters { get; set; }

        public int? UnclassifiedMeters { get; set; }

        /// <summary>
        /// Elevation Profile optimized for this track (no alignments), some other info integrated (TBD)
        /// </summary>
        public Guid? ElevationProfileImageID { get; set; }

        public Blob ElevationProfileImage { get; set; }

        public bool TryGetMinimumChallenge(out ChallengeLevel level)
        {
            level = ChallengeLevel.Basic;

            if ((this.DistanceMeters.HasValue) && (this.MinimumAltitude.HasValue) && (this.MaximumAltitude.HasValue))
            {
                int delta = this.MaximumAltitude.Value - this.MinimumAltitude.Value;
                if ((this.DistanceMeters > 30000) && (delta > 800))
                {
                    level = ChallengeLevel.Proficiency;
                    return true;
                }

                if ((this.DistanceMeters > 30000) && (delta > 800))
                {
                    level = ChallengeLevel.Proficiency;
                    return true;
                }

                if ((this.DistanceMeters > 20000) && (delta > 500))
                {
                    level = ChallengeLevel.Advanced;
                    return true;
                }

                if ((this.DistanceMeters > 10000) && (delta > 250))
                {
                    level = ChallengeLevel.Intermediate;
                    return true;
                }

                level = ChallengeLevel.Basic;
                return true;
            }
            return false;
        }

        public Guid? ElevationProfile_Basic_ImageID { get; set; }

        public Guid? ElevationProfile_Intermediate_ImageID { get; set; }

        public Guid? ElevationProfile_Advanced_ImageID { get; set; }

        public Guid? ElevationProfile_Proficiency_ImageID { get; set; }

        public List<Event> Events { get; set; } = new List<Event>();

        public Place StartPlace { get; set; }

        public Place EndPlace { get; set; }

        public Guid? StartPlaceID { get; set; }

        public Guid? EndPlaceID { get; set; }

        /// <summary>
        /// serialized array/list of coordinates
        /// </summary>
        public string BoundingBox { get; set; }
    }
}
