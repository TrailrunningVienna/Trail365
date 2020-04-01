namespace Trail365.Configuration
{
    public class Features
    {
        /// <summary>
        /// User can upload single GPX File via UI (entry button on view UserProfile)
        /// </summary>
        public bool UserTrailUpload { get; set; }

        /// <summary>
        /// Admin can upload single GPX File via UI (entry button on view UserProfile)
        /// SAME feature like UserTrailUpload, but restricted to Admins!
        /// </summary>
        public bool AdminTrailUpload { get; set; }

        /// <summary>
        /// MainMenu Item
        /// </summary>
        public bool Trails { get; set; } = true;

        /// <summary>
        /// Admin can create/modify/delete stories
        /// </summary>
        public bool AdminStories { get; set; } = true;

        /// <summary>
        /// User can create/modify/delete stories
        /// </summary>
        public bool UserStories { get; set; } = false;

        /// <summary>
        /// Story (Stream) Frontend Main Menu Area
        /// </summary>
        public bool Stories { get; set; } = true;

        /// <summary>
        /// Events (Stream) Frontend Main Menu Area
        /// </summary>
        public bool Events { get; set; } = false;

        /// <summary>
        /// Frontend User Profile, editable for the User
        /// </summary>
        public bool UserProfile { get; set; } = false;

        /// <summary>
        /// Switch on/off login Menu Item
        /// </summary>
        public bool Login { get; set; } = true;

        public bool TrailAnalyzer { get; set; } = false;

        public bool ShareOnFacebook { get; set; } = true;
    }
}
