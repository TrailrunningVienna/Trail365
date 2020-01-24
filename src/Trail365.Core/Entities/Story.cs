using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    /// <summary>
    ///
    /// </summary>
    public class Story
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public AccessLevel ListAccess { get; set; } = AccessLevel.User;

        [Required]
        public string Name { get; set; }

        public string Excerpt { get; set; }

        public Blob CoverImage { get; set; }

        public Guid? CoverImageID { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public StoryStatus Status { get; set; } = StoryStatus.Draft;

        public DateTime? PublishedUtc { get; set; }

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        public List<StoryBlock> StoryBlocks { get; set; } = new List<StoryBlock>();

        public Blob GetCoverImageOrDefault()
        {
            if (this.CoverImageID.HasValue)
            {
                if (this.CoverImage == null) throw new InvalidOperationException("CoverImage not included on EF Context");
                return this.CoverImage;
            }
            return null;
        }
    }
}
