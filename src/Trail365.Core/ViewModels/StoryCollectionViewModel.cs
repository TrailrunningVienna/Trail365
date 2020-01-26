using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Trail365.ViewModels
{
    public class StoryCollectionViewModel
    {

        [HiddenInput]
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public List<StoryViewModel> Stories { get; set; } = new List<StoryViewModel>();

        public string SearchText { get; set; }

        public bool HasSearchText()
        {
            return !string.IsNullOrEmpty(this.SearchText);
        }
    }
}
