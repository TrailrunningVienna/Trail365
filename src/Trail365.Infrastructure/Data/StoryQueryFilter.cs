using System;
using System.Linq;
using Trail365.Entities;

namespace Trail365.Data
{
    public class StoryQueryFilter
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeBlocks">if true, images are included implicitely</param>
        /// <returns></returns>
        public static StoryQueryFilter GetByID(Guid id, bool includeBlocks)
        {
            return new StoryQueryFilter
            {
                StoryID = id,
                FilterByAllowedLevels = false,
                IncludeBlocks = includeBlocks
            };
        }

        public static StoryQueryFilter GetByID(Guid id, bool includeBlocks, AccessLevel[] accessLevels)
        {
            return new StoryQueryFilter
            {
                StoryID = id,
                FilterByAllowedLevels = true,
                IncludeBlocks = includeBlocks,
                AllowedLevels = accessLevels,
            };
        }

        /// <summary>
        /// Top N
        /// </summary>
        public int? Take { get; set; }

        public int? Skip { get; set; }
        public Guid? StoryID { get; set; }

        public static readonly StoryStatus[] PublishedOnlyExclusions = new StoryStatus[] { StoryStatus.Upload, StoryStatus.Draft };

        public AccessLevel[] AllowedLevels { get; private set; } = new AccessLevel[] { };

        public bool FilterByAllowedLevels { get; set; } = false;

        public StoryStatus[] ExcludedStatus { get; set; } = new StoryStatus[] { };

        public StoryStatus[] IncludedStatus { get; set; } = new StoryStatus[] { };

        public StoryQueryOrdering OrderBy { get; set; } = StoryQueryOrdering.None;

        public StoryQueryFilter()
        { }

        public StoryQueryFilter(AccessLevel[] allowedLevels, bool restrictToPublishedStoriesOnly) : this()
        {
            this.AllowedLevels = allowedLevels ?? throw new ArgumentNullException(nameof(allowedLevels));
            this.FilterByAllowedLevels = true;
            if (restrictToPublishedStoriesOnly)
            {
                this.ExcludedStatus = PublishedOnlyExclusions;
            }
        }

        public string SearchText { get; set; }

        /// <summary>
        /// Currently implicits Images on Blocks
        /// </summary>
        public bool IncludeBlocks { get; set; }

        internal string GetCommandText()
        {
            char listSep = ';';
            var allowedLevels = string.Join(listSep, this.AllowedLevels.Select(l => l.ToString()));
            var includedStati = string.Join(listSep, this.IncludedStatus.Select(s => s.ToString()));
            var excludedStati = string.Join(listSep, this.ExcludedStatus.Select(s => s.ToString()));
            return $"TopN={this.Take}, IncludeBlocks={this.IncludeBlocks}, StoryID='{this.StoryID}', SortOrder={this.OrderBy}, AllowedLevel={allowedLevels}, Take={this.Take}, Skip={this.Skip}, IncludedStatus={includedStati}, ExcludedStatus={excludedStati}, SearchText='{this.SearchText}'";
        }
    }
}
