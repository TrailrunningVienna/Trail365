using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.ViewModels
{
    public class TrailViewModel
    {
        public string GetHumanizedMetadata()
        {
            if ((this.DistanceKm.HasValue) && (this.Ascent.HasValue))
            {
                return $"{Convert.ToInt32(this.DistanceKm.Value).ToString()} km, {Convert.ToInt32(this.Ascent.Value).ToString()}HM (aufsteigend)";
            }
            if (this.DistanceKm.HasValue)
            {
                return $"{Convert.ToInt32(this.DistanceKm.Value).ToString()} km";
            }
            return string.Empty;
        }

        public bool ShowEditLink { get; set; }

        public bool ShowDownloadLink { get; set; }

        public bool HideName { get; set; }

        public bool HideExcerpt { get; set; }

        public LoginViewModel Login { get; set; } = new LoginViewModel();

        /// <summary>
        /// default to false;
        /// </summary>
        public bool NoConsent { get; set; }

        /// <summary>
        /// true if the calling request is coming from social media for scraping
        /// </summary>
        public bool Scraping { get; set; }

        public ChallengeLevel Challenge { get; set; } = ChallengeLevel.Advanced;

        public AccessLevel GpxDownloadAccess { get; set; }
        public AccessLevel ListAccess { get; set; }

        public bool CanDownload()
        {
            Guard.AssertNotNull(this.Login);
            return this.Login.CanDo(this.GpxDownloadAccess);
        }

        public bool CanEdit()
        {
            Guard.AssertNotNull(this.Login);
            return this.Login.IsAdmin || this.Login.IsModerator;
        }

        public Dictionary<string, string> CreateOpenGraphTags(IUrlHelper helper, Size imageSize, string appID)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var ogDict = new Dictionary<string, string>
            {
                {"og:url", helper.GetTrailDetailsUrl(this.ID,true)},
                {"og:type", "website"},
                {"og:title", $"{this.Name}".Trim()},
                {"og:description", $"{this.Description}".Trim()}
            };

            if (string.IsNullOrEmpty(this.PreviewUrl) == false)
            {
                ogDict.Add("og:image", this.PreviewUrl);
                if (!imageSize.IsEmpty)
                {
                    ogDict.Add("og:image:width", imageSize.Width.ToString());
                    ogDict.Add("og:image:height", imageSize.Height.ToString());
                }
            }
            if (!string.IsNullOrEmpty(appID))
            {
                ogDict.Add("fb:app_id", appID);
            }
            return ogDict;
        }

        public Guid ID { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        /// <summary>
        /// specially used in Track-Creation and upload scenarios
        /// </summary>
        public byte[] Gpx { get; set; }

        private string GetProfileUrlForChallenge(ChallengeLevel level)
        {
            switch (level)
            {
                case ChallengeLevel.Basic:
                    return this.ElevationProfile_Basic_Url;

                case ChallengeLevel.Intermediate:
                    return this.ElevationProfile_Intermediate_Url;

                case ChallengeLevel.Advanced:
                    return this.ElevationProfile_Advanced_Url;

                case ChallengeLevel.Proficiency:
                    return this.ElevationProfile_Proficiency_Url;

                default:
                    throw new NotImplementedException($"{nameof(ChallengeLevel)} '{level}' not implemented in '{nameof(this.GetProfileUrlForChallenge)}'");
            }
        }

        public string GetChallengeElevationProfileUrlOrDefault(ChallengeLevel level)
        {
            string s = this.GetProfileUrlForChallenge(level);
            if (string.IsNullOrEmpty(s)) return null;
            return s;
        }

        public string GetElevationProfileUrlOrDefault()
        {
            if (!string.IsNullOrEmpty(this.ElevationProfileUrl)) return this.ElevationProfileUrl;
            return null;
        }

        public string GetSmallestPreviewUrlOrDefault()
        {
            if (!string.IsNullOrEmpty(this.SmallPreviewUrl)) return this.SmallPreviewUrl;
            if (!string.IsNullOrEmpty(this.MediumPreviewUrl)) return this.MediumPreviewUrl;
            if (!string.IsNullOrEmpty(this.PreviewUrl)) return this.PreviewUrl;
            return null;
        }

        public string[] GetTrailPictures()
        {
            List<string> urls = new List<string>();
            string s;
            bool small = !this.Scraping;
            if (small)
            {
                s = this.GetSmallestPreviewUrlOrDefault();
            }
            else
            {
                s = this.GetBestPreviewUrlOrDefault();
            }

            if (string.IsNullOrEmpty(s) == false)
            {
                urls.Add(s);
            }

            return urls.ToArray();
        }

        public string GetBestPreviewUrlOrDefault()
        {
            if (!string.IsNullOrEmpty(this.PreviewUrl)) return this.PreviewUrl;
            if (!string.IsNullOrEmpty(this.MediumPreviewUrl)) return this.MediumPreviewUrl;
            if (!string.IsNullOrEmpty(this.SmallPreviewUrl)) return this.SmallPreviewUrl;
            return null;
        }

        public string GetMediumPreviewUrlOrDefault()
        {
            if (!string.IsNullOrEmpty(this.MediumPreviewUrl)) return this.MediumPreviewUrl;
            if (!string.IsNullOrEmpty(this.PreviewUrl)) return this.PreviewUrl;
            return null;
        }

        public string ElevationProfileUrl { get; set; }

        public string ElevationProfile_Basic_Url { get; set; }

        public string ElevationProfile_Intermediate_Url { get; set; }

        public string ElevationProfile_Advanced_Url { get; set; }

        public string ElevationProfile_Proficiency_Url { get; set; }

        public string PreviewUrl { get; set; }

        public string SmallPreviewUrl { get; set; }

        public string MediumPreviewUrl { get; set; }

        /// <summary>
        /// Local, not UTC!
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// local, not UTC
        /// </summary>
        public DateTime? Modified { get; set; }

        public string GetLastModifiedDateOrDefault()
        {
            DateTime? v = this.Modified;
            if (!v.HasValue)
            {
                v = this.Created;
            }
            return v.ToDateFormatForDefault();
        }

        public string Description { get; set; }

        /// <summary>
        /// Gpx is stored on external systems, this is the Link!
        /// </summary>
        public string GpxUrl { get; set; }

        public string Excerpt { get; set; }

        public double? DistanceKm { get; set; }

        public int? Ascent { get; set; }
        public int? Descent { get; set; }

        public string GetTrailAnalyzerUrl(IUrlHelper url)
        {
            return url.GetTrailExplorerUrlOrDefault(this.GpxUrl);
        }
    }
}
