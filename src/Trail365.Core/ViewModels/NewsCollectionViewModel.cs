using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Trail365.ViewModels
{
    public class NewsCollectionViewModel
    {
        public List<NewsViewModel> News { get; set; } = new List<NewsViewModel>();

        [HiddenInput]
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public bool NoConsent { get; set; }

        /// <summary>
        /// true if the calling request is coming from social media for scraping
        /// </summary>
        public bool Scraping { get; set; }

        public Dictionary<string, string> CreateOpenGraphTags(IUrlHelper helper, string appID = null)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var ogDict = new Dictionary<string, string>
            {
                {"og:url", helper.GetNewsUrl(true,true)},
                {"og:type", "website"},
                {"og:title", $"Trail365".Trim()},
                {"og:description", $"Die Laufplattform für begeisterte Trailrunner".Trim()}
            };

            ogDict.Add("og:image", helper.AbsoluteUrl("icon-600x315.png"));
            ogDict.Add("og:image:width", 600.ToString());
            ogDict.Add("og:image:height", 315.ToString());

            if (!string.IsNullOrEmpty(appID))
            {
                ogDict.Add("fb:app_id", appID);
            }
            return ogDict;
        }
    }
}
