using System;
using Trail365.Entities;

namespace Trail365.DTOs
{
    public class StoryBlockDto
    {
        public BlobDto Image { get; set; }
        public Guid ID { get; set; } = Guid.NewGuid();
        public StoryBlockType BlockType { get; set; }
        public string RawContent { get; set; }
        public int SortOrder { get; set; } = 0;
        public int BlockTypeGroup { get; set; } = 0;

    }
}
