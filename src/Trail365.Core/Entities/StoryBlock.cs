using System;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    public class StoryBlock
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? ImageID { get; set; }

        public Blob Image { get; set; }

        [Required]
        public Guid StoryID { get; set; }

        public Story Story { get; set; }

        public string RawContent { get; set; }

        public StoryBlockType BlockType { get; set; } = StoryBlockType.Text;

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// multiple blocks with the same BlockType (Image) and the same group are handled like a album
        /// group changes are handled like album changes
        /// </summary>
        public int BlockTypeGroup { get; set; } = 0;

    }
}
