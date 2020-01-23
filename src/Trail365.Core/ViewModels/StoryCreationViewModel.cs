using System;
using System.Collections.Generic;

namespace Trail365.ViewModels
{
    public class StoryCreationViewModel
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public List<StoryBlockFileViewModel> FileInfos { get; set; } = new List<StoryBlockFileViewModel>();

        public LoginViewModel Login { get; set; } = new LoginViewModel();
    }
}
