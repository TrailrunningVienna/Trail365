using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class PlaceBackendViewModel
    {
        public Place ApplyChangesTo(Place item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.Name = this.Name;
            item.CountryTwoLetterISOCode = this.CountryTwoLetterISOCode;
            item.City = this.City;
            item.Radius = this.Radius;
            item.Zip = this.Zip;
            item.Longitude = this.Longitude;
            item.Latitude = this.Latitude;
            item.ExternalID = this.ExternalID;
            item.ExternalSource = this.ExternalSource;
            item.MeetingPoint = this.MeetingPoint;
            item.IsCityPartOfTheName = this.IsCityPartOfTheName;

            if (string.IsNullOrEmpty(this.CountryTwoLetterISOCode) == false)
            {
                if (this.CountryTwoLetterISOCode == "??")
                {
                    item.CountryTwoLetterISOCode = null;
                    item.Country = null;
                }
                else
                {
                    var finding = ISO3166.Country.List.Where(c => string.Compare(c.TwoLetterCode, this.CountryTwoLetterISOCode, ignoreCase: true) == 0).FirstOrDefault();

                    if (finding != null)
                    {
                        item.Country = finding.Name;
                    }
                }
            }
            return item;
        }

        public List<Tuple<string, string>> References { get; set; } = new List<Tuple<string, string>>();

        public Guid ID { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string Zip { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        /// <summary>
        /// Country (Full Name) not used as property on this ViewModel!
        /// </summary>
        [Display(Name = "Land")]
        public string CountryTwoLetterISOCode { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public int? Radius { get; set; }

        [Display(Name = "ID (extern)")]
        public string ExternalID { get; set; }

        [Display(Name = "Quelle (extern)")]
        public string ExternalSource { get; set; }

        public static PlaceBackendViewModel CreateFromPlace(Place from)
        {
            var vm = new PlaceBackendViewModel
            {
                ID = from.ID,
                Name = from.Name,
                Zip = from.Zip,
                City = from.City,
                CountryTwoLetterISOCode = from.CountryTwoLetterISOCode,
                Longitude = from.Longitude,
                Latitude = from.Latitude,
                Radius = from.Radius,
                Created = from.CreatedUtc.ToLocalTime(),
                Modified = from.ModifiedUtc?.ToLocalTime(),
                ExternalSource = from.ExternalSource,
                ExternalID = from.ExternalID,
                IsCityPartOfTheName = from.IsCityPartOfTheName,
                MeetingPoint = from.MeetingPoint,
            };
            return vm;
        }

        public bool IsCityPartOfTheName { get; set; }

        [Display(Name = "Treffpunkt")]
        public string MeetingPoint { get; set; }
    }
}
