using MSS.GraphQL.Facebook;

namespace Trail365.Web
{
    public class FacebookEventData
    {
        public string ExternalSource { get; set; }
        public FacebookEventDescriptor[] Events { get; set; }
    }
}
