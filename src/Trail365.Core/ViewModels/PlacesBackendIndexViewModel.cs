using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trail365.ViewModels
{
    public class PlacesBackendIndexViewModel : DatapagerViewModel
    {
        public List<PlaceBackendViewModel> Places { get; set; }

        public string SearchText { get; set; }

        [Display(Name = "ID (extern)")]
        public string ExternalID { get; set; }

        [Display(Name = "Quelle (extern)")]
        public string ExternalSource { get; set; }

        [Display(Name = "Land")]
        public string CountryTwoLetterISOCode { get; set; }
    }
}
