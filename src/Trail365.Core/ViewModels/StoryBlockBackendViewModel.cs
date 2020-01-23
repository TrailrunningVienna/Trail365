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
    }
}
