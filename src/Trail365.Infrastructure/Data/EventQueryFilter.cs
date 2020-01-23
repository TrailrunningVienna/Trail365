using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.Entities;

namespace Trail365.Data
{
    public class EventQueryFilter
    {
        private System.Linq.Expressions.Expression<Func<Event, bool>> Where { get; set; }

        public static readonly EventStatus[] PublishedOnlyExclusions = new EventStatus[] { EventStatus.Canceled, EventStatus.Draft };

        public EventQueryOrdering OrderBy { get; set; } = EventQueryOrdering.None;

        public EventQueryFilter()
        { }

        public DateTime? StartTimeUtcMinValue { get; set; }

        public EventQueryFilter(AccessLevel[] allowedLevels, bool restrictToPublishedEventsOnly) : this()
        {
            this.AllowedLevels = allowedLevels ?? throw new ArgumentNullException(nameof(allowedLevels));
            this.FilterByAllowedLevels = true;
            if (restrictToPublishedEventsOnly)
            {
                this.ExcludedStatus = PublishedOnlyExclusions;
            }
        }

        internal string GetCommandText()
        {
            var allowedLevels = string.Join('_', this.AllowedLevels.Select(l => l.ToString()));
            return $"TopN={this.TopN}, IncludePlaces={this.IncludePlaces}, IncludeImages={this.IncludeImages}, IncludeTrails={this.IncludeTrails}, EventID='{this.EventID}', OwnerID='{this.OwnerID}',OrderBy={this.OrderBy}, AllowedLevels={allowedLevels}";
        }

        public bool IncludePlaces { get; set; }

        public bool IncludeImages { get; set; }

        /// <summary>
        /// Implicits Blob for Gpx!
        /// </summary>
        public bool IncludeTrails { get; set; }

        public int? TopN { get; set; }

        public Guid? EventID { get; set; }

        public Guid? OwnerID { get; set; }

        public AccessLevel[] AllowedLevels { get; private set; } = new AccessLevel[] { };

        public bool FilterByAllowedLevels { get; set; } = false;

        public EventStatus[] ExcludedStatus { get; set; } = new EventStatus[] { };

        public List<Guid> ExcludedEvents { get; set; } = new List<Guid>();
    }
}
