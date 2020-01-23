using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Trail365.ViewModels
{
    public class StoryCollectionViewModel
    {
        public string GetStoryPreviewPartialName()
        {
            string partialName = "_StoryAsListItem"; //_EventAsGridItem

            //if (this.AsGrid)
            //{
            //    partialName = "_TrailPreviewGrid";
            //}
            return partialName;
        }

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
