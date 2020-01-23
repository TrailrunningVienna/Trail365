using System;
using System.ComponentModel.DataAnnotations;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class EventBackendViewModel
    {
        public Event ApplyChangesTo(Event item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.Description = this.Description;
            item.InternalDescription = this.InternalDescription;
            item.Name = this.Name;
            item.ListAccess = this.ReadAccess;
            item.Excerpt = this.Excerpt;
            item.ModifiedUtc = DateTime.UtcNow;
            item.OrganizerPermalink = this.OrganizerPermalink;

            item.PlaceID = this.PlaceID;
            item.EndPlaceID = this.EndPlaceID;
            item.TrailID = this.TrailID;

            item.Place = null;
            item.Trail = null;
            item.EndPlace = null;
            item.FullDayEvent = this.FullDayEvent;
            item.Status = this.Status;

            if (item.PlaceID == Guid.Empty)
            {
                item.PlaceID = null;
            }

            if (item.EndPlaceID == Guid.Empty)
            {
                item.EndPlaceID = null;
            }

            if (item.TrailID == Guid.Empty)
            {
                item.TrailID = null;
            }

            item.StartTimeUtc = this.StartTime.ToUniversalTime();
            item.EndTimeUtc = this.EndTime.ToUniversalTime();

            if (this.HasDescriptionImportLock)
            {
                item.DescriptionLock |= ContentLock.ImportLock;
            }
            else
            {
                item.DescriptionLock &= ~ContentLock.ImportLock;
            }

            if (this.HasPlaceImportLock)
            {
                item.PlaceLock |= ContentLock.ImportLock;
            }
            else
            {
                item.PlaceLock &= ~ContentLock.ImportLock;
            }

            return item;
        }

        public static EventBackendViewModel CreateFromEntity(Event from)
        {
            var vm = new EventBackendViewModel
            {
                ID = from.ID,
                Name = from.Name,
                Created = from.CreatedUtc.ToLocalTime(),
                Modified = from.ModifiedUtc.ToLocalTime(),
                Description = from.Description,
                CoverImageUrl = from.CoverImage?.Url,
                ReadAccess = from.ListAccess,
                Excerpt = from.Excerpt,
                OrganizerPermalink = from.OrganizerPermalink,
                StartTime = from.StartTimeUtc.ToLocalTime(),
                EndTime = from.EndTimeUtc.ToLocalTime(),
                CountryTwoLetterISOCode = from.Place?.CountryTwoLetterISOCode,
                City = from.Place?.City,
                FullDayEvent = from.FullDayEvent,
                Status = from.Status,
                InternalDescription = from.InternalDescription,
                ExternalSource = from.ExternalSource,
                ExternalID = from.ExternalID
            };

            vm.HasDescriptionImportLock = from.DescriptionLock.HasFlag(ContentLock.ImportLock);
            vm.HasPlaceImportLock = from.PlaceLock.HasFlag(ContentLock.ImportLock);

            if (from.PlaceID.HasValue)
            {
                vm.PlaceID = from.PlaceID.Value;
            }

            if (from.EndPlaceID.HasValue)
            {
                vm.EndPlaceID = from.EndPlaceID.Value;
            }

            if (from.TrailID.HasValue)
            {
                vm.TrailID = from.TrailID.Value;
            }

            return vm;
        }

        public string ExternalSource { get; set; }

        public string ExternalID { get; set; }

        [Display(Name = "Import-Sperre f. Beschreibung")]
        public bool HasDescriptionImportLock { get; set; }

        [Display(Name = "Import-Sperre f. Place")]
        public bool HasPlaceImportLock { get; set; }

        public EventStatus Status { get; set; }

        public bool FullDayEvent { get; set; }

        [Display(Name = "Land")]
        public string CountryTwoLetterISOCode { get; private set; }

        [Display(Name = "Ort")]
        public string City { get; private set; }

        /// <summary>
        /// Nullable via GUID.Empty
        /// </summary>
        [Display(Name = "Ort")]
        public Guid PlaceID { get; set; } = Guid.Empty;

        /// <summary>
        /// Nullable via GUID.Empty
        /// </summary>
        [Display(Name = "Ort (Ende)")]
        public Guid EndPlaceID { get; set; } = Guid.Empty;

        [Display(Name = "Track")]
        public Guid TrailID { get; set; } = Guid.Empty;

        public Guid ID { get; set; }

        [Display(Name = "Cover Image")]
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// link to the Event provided by the Organizer
        /// </summary>
        [Display(Name = "Organizer (Link)")]
        public string OrganizerPermalink { get; set; }

        [Display(Name = "Read Access")]
        public AccessLevel ReadAccess { get; set; }

        public string Name { get; set; }

        public DateTime? Modified { get; set; }

        public DateTime Created { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Description { get; set; }

        public string InternalDescription { get; set; }

        public string Excerpt { get; set; }
    }
}
