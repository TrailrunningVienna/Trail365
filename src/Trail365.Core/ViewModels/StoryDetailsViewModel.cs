using System;
using System.Collections.Generic;

namespace Trail365.ViewModels
{
    public class StoryDetailsViewModel : DatapagerViewModel
    {
        public Guid ID { get; set; }
        public List<StoryBlockViewModel> StoryBlocks { get; set; }
    }
}
