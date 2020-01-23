using System.Collections.Generic;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class StoriesBackendIndexViewModel : DatapagerViewModel
    {
        public List<StoryBackendViewModel> Stories { get; set; }

        public string SearchText { get; set; }

        public StoryStatus? Status { get; set; }

        //[Display(Name = "ID (extern)")]
        //public string ExternalID { get; set; }

        //[Display(Name = "Quelle (extern)")]
        //public string ExternalSource { get; set; }

        //[Display(Name = "Land")]
        //public string CountryTwoLetterISOCode { get; set; }
    }
}
