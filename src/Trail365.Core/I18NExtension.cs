using System;

namespace Trail365
{
    public static class I18NExtension
    {
        public static DateTime? ToLocalTime(this DateTime? utc)
        {
            if (utc.HasValue == false)
            {
                return null;
            }
            else
            {
                return utc.Value.ToLocalTime();
            }
        }

        public static DateTime? ToUniversalTime(this DateTime? localTime)
        {
            if (localTime.HasValue == false)
            {
                return null;
            }
            return localTime.Value.ToUniversalTime();
        }
    }
}
