using Microsoft.Extensions.Logging;

namespace Trail365
{
    public static class ApplicationEventId
    {
        public static readonly EventId ApplicationStarted = new EventId(2001, nameof(ApplicationStarted));
        public static readonly EventId ApplicationStopping = new EventId(2002, nameof(ApplicationStopping));
        public static readonly EventId ApplicationStopped = new EventId(2003, nameof(ApplicationStopped));
    }
}
