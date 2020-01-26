using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.Entities;

namespace Trail365.Data
{
    public class EventQueryFilter : QueryFilter
    {

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
            return $"TopN={this.Take}, IncludePlaces={this.IncludePlaces}, IncludeImages={this.IncludeImages}, IncludeTrails={this.IncludeTrails}, EventID='{this.EventID}', OwnerID='{this.OwnerID}',OrderBy={this.OrderBy}, AllowedLevels={allowedLevels}";
        }

        public bool IncludePlaces { get; set; }

        public bool IncludeImages { get; set; }

        /// <summary>
        /// Implicits Blob for Gpx!
        /// </summary>
        public bool IncludeTrails { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }

        public Guid? EventID { get; set; }

        public Guid? OwnerID { get; set; }

        public AccessLevel[] AllowedLevels { get; private set; } = new AccessLevel[] { };

        public bool FilterByAllowedLevels { get; set; } = false;

        public EventStatus[] ExcludedStatus { get; set; } = new EventStatus[] { };

        public List<Guid> ExcludedEvents { get; set; } = new List<Guid>();


        public override string GetCacheKey()
        {
            string[] levelArgs = new string[] { "nolevelfilter" };
            var filter = this;
            if (filter.FilterByAllowedLevels)
            {
                levelArgs = filter.AllowedLevels.OrderBy(l => l).Select(l => l.ToString().ToLowerInvariant()).ToArray();
            }

            var excludedStatus = filter.ExcludedStatus.OrderBy(e => e).Select(e => $"exST:{e.ToString()}".ToLowerInvariant()).ToArray();
            var excludedEvent = filter.ExcludedEvents.OrderBy(e => e).Select(e => $"exEV:{e.ToString()}".ToLowerInvariant()).ToArray();

            var coreArgs = new string[] {
                    filter.EventID.HasValue ? filter.EventID.ToString():"noid",
                    filter.OwnerID.HasValue ? filter.OwnerID.ToString():"noownerid",

                    filter.Skip.HasValue ? filter.Skip.Value.ToString(): "noskip",
                    filter.StartTimeUtcMinValue.HasValue ? filter.StartTimeUtcMinValue.Value.ToString("o"): "nostarttime",
                    filter.Take.HasValue? filter.Take.ToString():"notake",
                    filter.IncludeTrails.ToString(),
                    filter.IncludePlaces.ToString(),
                    filter.IncludeImages.ToString(),
                    filter.OrderBy.ToString()
                    //,$"{filter.SearchText}"
            };
            return string.Join('|', coreArgs.Concat(levelArgs).Concat(excludedStatus).Concat(excludedEvent));
        }

    }
}
