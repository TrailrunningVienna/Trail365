using System;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class StoryBlockBackendViewModel
    {
        public Guid ID { get; set; }

        public string Content { get; set; }

        public StoryBlockType BlockType { get; set; }

        public string Url { get; set; }

        public int SortOrder { get; set; }

        /// <summary>
        /// multiple blocks with the same BlockType (Image) and the same group are handled like a album
        /// group changes are handled like album changes
        /// </summary>
        public int BlockTypeGroup { get; set; } = 0;
    }
}
