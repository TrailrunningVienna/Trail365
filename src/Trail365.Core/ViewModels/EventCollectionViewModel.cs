using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Trail365.ViewModels
{
    public class EventCollectionViewModel
    {
        public bool NoConsent { get; set; }

        /// <summary>
        /// true if the calling request is coming from social media for scraping
        /// </summary>
        public bool Scraping { get; set; }


        [HiddenInput]
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public List<EventViewModel> Events { get; set; } = new List<EventViewModel>();

        public string SearchText { get; set; }

        public bool HasSearchText()
        {
            return !string.IsNullOrEmpty(this.SearchText);
        }
    }
}
