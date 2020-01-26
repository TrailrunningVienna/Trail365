using System;
using System.Collections.Generic;
using Trail365.Entities;

namespace Trail365.DTOs
{
    public class StoryDto
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        public AccessLevel ListAccess { get; set; } = AccessLevel.User;

        public string Name { get; set; }

        public string Excerpt { get; set; }

        public Guid? CoverImageID { get; set; }

        public List<StoryBlockDto> StoryBlocks { get; set; } = new List<StoryBlockDto>();
    }
}
