using System.Collections.Generic;

namespace Trail365.ViewModels
{
    public class TrailCollectionViewModel
    {
        public List<TrailNewsViewModel> Items { get; set; }

        public LoginViewModel Login { get; set; } = new LoginViewModel();
    }
}
