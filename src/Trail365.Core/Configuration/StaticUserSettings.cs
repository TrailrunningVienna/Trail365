using System;

namespace Trail365.Configuration
{
    public class StaticUserSettings
    {
        public Guid UserID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string EMail { get; set; }
        public string NameIdentifier { get; set; }
        public string Roles { get; set; }
        public bool ShouldNotLogin { get; set; }
    }
}
