using System;

namespace Trail365.Web
{
    public static class DateTimeHelper
    {
        public static string GetGeburtsdatumMinValueIsoString()
        {
            return new DateTime(DateTime.Now.Year - 99, 1, 1).ToString("yyyy-MM-dd");
        }

        public static string GetGeburtsdatumMaxValueIsoString()
        {
            return new DateTime(DateTime.Now.Year - 1, 12, 31).ToString("yyyy-MM-dd");
        }
    }
}
