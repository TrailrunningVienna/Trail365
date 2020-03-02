using System;

namespace Trail365.ViewModels
{
    public class StoryRequestViewModel
    {
        public Guid? ID { get; set; }
        public bool? NoConsent { get; set; }
        public bool? Scraping { get; set; }
        public bool? IgnoreCache { get; set; }
    }
}
