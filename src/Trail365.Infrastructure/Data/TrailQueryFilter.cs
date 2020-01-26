using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.Entities;

namespace Trail365.Data
{
    public class TrailQueryFilter : QueryFilter
    {
        public int? Skip { get; set; }
        public Guid? TrailID { get; set; }

        private static readonly char[] SearchSplitDelimiter = new char[] { ' ' };

        public static TrailQueryFilter GetByID(Guid id, bool includeGpxBlob)
        {
            return new TrailQueryFilter
            {
                TrailID = id,
                FilterByAllowedLevels = false,
                IncludeGpxBlob = includeGpxBlob
            };
        }

        public bool HasSearchText()
        {
            return !string.IsNullOrEmpty(this.SearchText);
        }

        public TrailQueryOrdering OrderBy { get; set; } = TrailQueryOrdering.None;

        public AccessLevel[] AllowedLevels { get; private set; } = new AccessLevel[] { };

        public bool FilterByAllowedLevels { get; private set; } = false;

        public TrailQueryFilter()
        { }

        public TrailQueryFilter(AccessLevel[] allowedLevels) : this()
        {
            this.AllowedLevels = allowedLevels ?? throw new ArgumentNullException(nameof(allowedLevels));
            this.FilterByAllowedLevels = true;
        }

        public Guid? OwnerID { get; set; }
        /// <summary>
        /// Top n
        /// </summary>
        public int? Take { get; set; }
        public bool IncludePlaces { get; set; }

        /// <summary>
        /// currently only the minimum: "ElevationProfileImage" and "SmallPreview"
        /// </summary>
        public bool IncludeImages { get; set; }
        public bool IncludeGpxBlob { get; set; }

        public string GetCommandText()
        {
            List<string> items = new List<string>
            {
                this.Take.HasValue ? $"Top {this.Take.Value.ToString()}" : "?",
                this.Skip.HasValue ? $"Skip {this.Skip.Value.ToString()}" : "?",
                this.TrailID.HasValue ? $"OwnerID {this.TrailID.Value.ToString()}" : "?",
                $"{this.IncludeImages}",
                $"{this.IncludePlaces}",
                $"{this.IncludeGpxBlob}",
                $"{this.SearchText}",
                string.Join(", ", this.AllowedLevels.Select(l => l.ToString())),
                this.OwnerID.HasValue ? $"OwnerID {this.OwnerID.Value.ToString()}" : "?"
            };
            return string.Join('|', items);
        }

        public string SearchText { get; set; }

        public string[] GetSearchTextSplitsInLowerCase()
        {
            if (!this.HasSearchText()) return new string[] { };
            var result = this.SearchText.Split(SearchSplitDelimiter, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).Distinct();
            return result.ToArray();
        }
    }
}

;
