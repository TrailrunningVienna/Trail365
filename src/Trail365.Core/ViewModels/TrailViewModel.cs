using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.ViewModels
{
    /// <summary>
    /// this VM should only be used for unidirectional (Output only) bindings
    /// </summary>
    public class TrailViewModel : TrailViewModelBase
    {
        public bool HasClassifications => this.UnpavedTrailMeters.HasValue || this.UnclassifiedMeters.HasValue;

        private string GetClassifiedInKm(int? meters)
        {
            if (meters.HasValue)
            {
                double km = Math.Round((double) meters.Value / 1000,1);
                return km.ToString("#0.0");
            }
            else
            {
                return "N/A";
            }
        }

        public string UnpavedTrail => GetClassifiedInKm(this.UnpavedTrailMeters);

        public string PavedRoad => GetClassifiedInKm(this.PavedRoadMeters);

        public string AsphaltedRoad => GetClassifiedInKm(this.AsphaltedRoadMeters);

        public string Unclassified => GetClassifiedInKm(this.UnclassifiedMeters);

        public string BBox { get; set; }

        /// <summary>
        /// pure trail
        /// </summary>
        public int? UnpavedTrailMeters { get; set; }

        /// <summary>
        /// typically forestry road
        /// </summary>
        public int? PavedRoadMeters { get; set; }

        public int? AsphaltedRoadMeters { get; set; }

        public int? UnclassifiedMeters { get; set; }

        public string PreviewUrl { get; set; }

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

        public bool ShowChallenge { get; set; } = true;

        public bool HideName { get; set; }

        public bool HideExcerpt { get; set; }


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
            if (string.IsNullOrEmpty(this.GpxUrl)) return false;
            return this.Login.CanDo(this.GpxDownloadAccess);
        }

        public bool CanEdit()
        {
            Guard.AssertNotNull(this.Login);
            return this.Login.IsAdmin || this.Login.IsModerator;
        }


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

        /// <summary>
        /// Gpx is stored on external systems, this is the Link!
        /// </summary>
        public string GpxUrl { get; set; }

        /// <summary>
        /// gpx converted into geoJson and some classifications applied!
        /// </summary>
        public string AnalyzerUrl { get; set; }

        /// <summary>
        /// proposed filename on the client side download (html "download" attribute)
        /// </summary>
        public string GpxDownloadFileName { get; set; }
        /// <summary>
        /// download access to the gpx file via self hosted file delivery to prevent CORS problems and to inject download filename
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="url"></param>
        /// <param name="downloadAttribute"></param>
        public void GetDownloadUrl(IUrlHelper helper,string containerName, out string url, out string downloadAttribute)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentNullException(nameof(containerName));
            var refUrl = new Uri(this.GpxUrl);
            var baseUrl = helper.GetStorageProxyBaseUrl();
            string relativeUrl = refUrl.PathAndQuery.Substring(containerName.Length + 1);
            url = baseUrl + relativeUrl;
            downloadAttribute = this.GpxDownloadFileName;
        }

        public string GetTrailAnalyzerUrl(string trailExplorerBaseUrl, IUrlHelper url)
        {
            return url.GetTrailExplorerUrlOrDefault(trailExplorerBaseUrl, this.AnalyzerUrl, ExplorerMapStyle.Outdoor, false,this.BBox);
        }
    }
}
