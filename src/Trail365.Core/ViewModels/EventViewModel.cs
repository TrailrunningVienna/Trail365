using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.ViewModels
{
    public class EventViewModel
    {

        public bool IsCircle
        {
            get
            {
                if ((this.Place == null) || this.EndPlace == null) return false;
                return (this.Place.ID == this.EndPlace.ID);
            }
        }

        public bool HasEnabledPlaceEditLink()
        {
            if (this.Place != null)
            {
                if (this.Place.ShowEditLink) return true;
            }
            if (this.EndPlace != null)
            {
                if (this.EndPlace.ShowEditLink) return true;
            }
            return false;
        }

        public EventViewModel EnableEditLinkForPlaces()
        {
            if (this.Place != null)
            {
                this.Place.ShowEditLink = true;
            }

            if (this.EndPlace != null)
            {
                this.EndPlace.ShowEditLink = true;
            }
            return this;
        }

        public EventViewModel EnableEditLinkForTrail()
        {
            if (this.Trail != null)
            {
                this.Trail.ShowEditLink = true;
            }
            return this;
        }

        public EventViewModel EnableDownloadLinkForTrail()
        {
            if (this.Trail != null)
            {
                this.Trail.ShowDownloadLink = true;
            }
            return this;
        }

        public string DetailsUrl { get; set; }

        public string CoverImageUrl { get; set; }

        public Size CoverImageSize { get; set; } = Size.Empty;

        public PlaceViewModel Place { get; set; }

        public PlaceViewModel EndPlace { get; set; }

        public bool NoConsent { get; set; }

        public bool FullDayEvent { get; set; }

        /// <summary>
        /// ONLY DATE (part) of the first day (included) for a full (multi-day-event)
        /// Single-day event: date (not time)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// ONLY DATE (part) of the last day (included) for a full (multi-day-event)
        /// Single-day event: date or null!
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Date and Time
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Date and time
        /// </summary>
        public DateTime? EndTime { get; set; }

        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public ChallengeLevel Challenge { get; set; } = ChallengeLevel.Advanced;

        public AccessLevel ListAccess { get; set; }

        public bool CanEdit()
        {
            Guard.AssertNotNull(this.Login);
            return this.Login.IsAdmin || this.Login.IsModerator;
        }

        public Dictionary<string, string> CreateOpenGraphTags(IUrlHelper helper, string appID)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var ogDict = new Dictionary<string, string>
            {
                {"og:url", helper.GetEventUrl(this.ID,true, true)},
                {"og:type", "website"},
                {"og:title", $"{this.Name}".Trim()},
                {"og:description", $"{this.Excerpt}".Trim()}
            };


            if (string.IsNullOrEmpty(this.CoverImageUrl) == false)
            {
                ogDict.Add("og:image", this.CoverImageUrl);
                var imageSize = this.CoverImageSize;

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

        public string GetHumanizedPlaceOrDefault(string defaultValue = "")
        {
            if (this.Place != null) return this.Place.GetHumanizedName();
            return defaultValue;
        }

        public string GetHumanizedEndPlaceOrDefault(string defaultValue = "")
        {
            if (this.EndPlace != null) return this.EndPlace.GetHumanizedName();
            return defaultValue;
        }

        public string GetMeetingPointOrDefault()
        {
            if (this.Place != null) return this.Place.MeetingPoint;
            return null;
        }

        public string GetHumanizedStartEndTime()
        {
            if (this.StartTime.HasValue == false)
            {
                return string.Empty;
            }
            string from;
            string to;
            if (this.FullDayEvent)
            {
                from = this.StartTime.Value.ToLongDateString();
                if ((this.EndTime.HasValue == false) || (this.EndTime.Value == this.StartTime.Value))
                {
                    return from;
                }
                to = this.EndTime.Value.ToLongDateString();
            }
            else
            {
                from = $"{this.StartTime.Value.ToShortDateString()} {this.StartTime.Value.ToShortTimeString()}";
                if (this.EndTime.HasValue == false)
                {
                    return from;
                }

                if (this.StartTime.Value.Date == this.EndTime.Value.Date)
                {
                    //same day
                    return from + $" - {this.EndTime.Value.ToShortTimeString()}";
                }
                to = $"{this.EndTime.Value.ToShortDateString()} {this.StartTime.Value.ToShortTimeString()}";
            }

            return $"{from} bis {to}";
        }

        public string GetHumanizedStartEndDate()
        {
            if (this.StartDate.HasValue == false)
            {
                return string.Empty;
            }
            string from;
            string to;
            if (this.FullDayEvent)
            {
                from = this.StartDate.Value.ToLongDateString();
                if ((this.EndDate.HasValue == false) || (this.EndDate.Value == this.StartDate.Value))
                {
                    return from;
                }
                to = this.EndDate.Value.ToLongDateString();
            }
            else
            {
                from = $"{this.StartDate.Value.ToShortDateString()} {this.StartDate.Value.ToLongTimeString()}";
                if (this.EndDate.HasValue == false)
                {
                    return from;
                }

                if (this.StartDate.Value.Date == this.EndDate.Value.Date)
                {
                    //same day
                    return from + $" - {this.EndDate.Value.ToLongTimeString()}";
                }
                to = $"{this.EndDate.Value.ToShortDateString()} {this.StartDate.Value.ToLongTimeString()}";
            }

            return $"{from} bis {to}";
        }

        public string Name { get; set; }

        public string Description { get; set; }

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

        public string Excerpt { get; set; }

        public TrailViewModel Trail { get; set; }
    }
}
