using System;

namespace Trail365.ViewModels
{
    public class PlaceViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public bool IsCityPartOfTheName { get; set; }
        public string MeetingPoint { get; set; }

        public string GetHumanizedName()
        {
            string proposed = this.Name;

            if (this.IsCityPartOfTheName == false)
            {
                if (!string.IsNullOrEmpty(this.City))
                {
                    return proposed + $" ({this.City})";
                }
            }
            return proposed;
        }

        public bool ShowEditLink { get; set; }
    }
}
