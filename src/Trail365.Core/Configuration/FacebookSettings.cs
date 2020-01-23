namespace Trail365.Configuration
{
    public class FacebookSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string ImporterId { get; set; }
        public string ImporterAccessToken { get; set; }
        public int ImporterDays { get; set; } = 5;
        public int ApiDelayMilliseconds { get; set; } = 1000;
    }
}
