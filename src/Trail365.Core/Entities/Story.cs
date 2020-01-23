using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public StoryStatus Status { get; set; } = StoryStatus.Draft;

        public DateTime? PublishedUtc { get; set; }

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        public List<StoryBlock> StoryBlocks { get; set; } = new List<StoryBlock>();

        public Blob GetCoverImageOrDefault()
        {
            StoryBlockType[] tryInOrder = new StoryBlockType[] { StoryBlockType.Title, StoryBlockType.Excerpt, StoryBlockType.Image, StoryBlockType.Text };

            var orderedBlocks = this.StoryBlocks.OrderBy(b => b.SortOrder);

            foreach (var blockType in tryInOrder)
            {
                var block = orderedBlocks.Where(sb => sb.BlockType == blockType && sb.Image != null).FirstOrDefault();
                if ((block != null) && (block.Image != null))
                {
                    return block.Image;
                }
            }
            return null;
        }
    }
}
