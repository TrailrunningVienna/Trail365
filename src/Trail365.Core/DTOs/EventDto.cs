using System;
using Trail365.Entities;

namespace Trail365.DTOs
{
    public class EventDto
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// userID of the owner, null if "system" or "undefined"
        /// </summary>
        public Guid? OwnerID { get; set; }

        public AccessLevel ListAccess { get; set; } = AccessLevel.User;

        public string Excerpt { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public string OrganizerPermalink { get; set; }

        public PlaceDto Place { get; set; }

        public Guid? PlaceID { get; set; }

        public string Description { get; set; }

        public BlobDto CoverImage { get; set; }

        public Guid? CoverImageID { get; set; }

        public string ExternalID { get; set; }

        public string ExternalSource { get; set; }
    }
}
