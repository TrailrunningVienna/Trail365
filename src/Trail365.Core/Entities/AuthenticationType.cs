using System.ComponentModel;

namespace Trail365.Entities
{
    public enum AuthenticationType
    {
        [Description("Trail365")]
        Cookies = 0,

        Facebook = 1,
        Google = 2,
        Apple = 3,
    }
}
