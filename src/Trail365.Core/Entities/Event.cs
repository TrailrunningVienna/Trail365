using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Trail365.Entities
{
    /// <summary>
    /// </summary>
    public class Event
    {

        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// userID of the owner, null if "system" or "undefined"
        /// </summary>
        public Guid? OwnerID { get; set; }

        public EventStatus Status { get; set; } = EventStatus.Default;

        public AccessLevel ListAccess { get; set; } = AccessLevel.User;

        public string Excerpt { get; set; }

        public string GetReferenceCaption()
        {
            return $"{this.Name} {this.GetDateTimestamp()} ({this.Status.ToDescription()})";
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        public static string CreateDateTimestamp(DateTime date)
        {
            string s = string.Format("{0:0000}{1:00}{2:00}", date.Year, date.Month, date.Day);
            Debug.Assert(s == s.Trim());
            return s;
        }

        public bool HasValidDates() => this.TryGetValidDates(out var _);

        public string GetDateTimestamp()
        {
            if (this.TryGetValidDates(out var t) == false)
            {
                throw new InvalidOperationException(string.Format("{0}: Date not available", nameof(this.GetDateTimestamp)));
            }
            return CreateDateTimestamp(t.Item1);
        }

        public bool TryGetValidDates(out Tuple<DateTime, DateTime> result)
        {
            result = null;
            if (this.StartTimeUtc.HasValue == false)
            {
                return false;
            }
            DateTime dt1 = this.StartTimeUtc.Value;
            DateTime dt2 = dt1; //the default case if EndDate is empty;
            if (this.EndTimeUtc.HasValue)
            {
                dt2 = this.EndTimeUtc.Value;
            }

            if (dt2 < dt1)
            {
                return false;
            }
            result = new Tuple<DateTime, DateTime>(dt1, dt2);
            return true;
        }

        public Event()
        {
        }

        public bool FullDayEvent { get; set; } = false;

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public string OrganizerPermalink { get; set; }

        public ContentLock DescriptionLock { get; set; } = ContentLock.None;

        public string Description { get; set; }

        public string InternalDescription { get; set; }

        public Place Place { get; set; }

        public ContentLock PlaceLock { get; set; } = ContentLock.None;

        public Guid? PlaceID { get; set; }

        public Guid? EndPlaceID { get; set; }

        [NotMapped]
        public Place EndPlace { get; set; }

        public List<Story> Stories { get; set; } = new List<Story>();

        /// <summary>
        /// can be null
        /// </summary>
        public Trail Trail { get; set; }

        public Guid? TrailID { get; set; }

        public Blob CoverImage { get; set; }

        public Guid? CoverImageID { get; set; }

        public string ExternalID { get; set; }

        public string ExternalSource { get; set; }
    }
}
