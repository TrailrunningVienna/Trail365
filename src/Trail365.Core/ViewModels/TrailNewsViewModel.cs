namespace Trail365.ViewModels
{
    public class TrailNewsViewModel : TrailViewModelBase
    {
        public PlaceViewModel StartPlace { get; set; }
        public PlaceViewModel EndPlace { get; set; }

        public string GetHumanizedPlaceOrDefault(string defaultValue = "")
        {
            if (this.StartPlace != null) return this.StartPlace.GetHumanizedName();
            return defaultValue;
        }

        public string GetMeetingPointOrDefault()
        {
            if (this.StartPlace != null) return this.StartPlace.MeetingPoint;
            return null;
        }

    }
}
