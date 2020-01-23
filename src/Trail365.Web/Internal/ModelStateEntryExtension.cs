namespace Trail365.Web
{
    public static class ModelStateEntryExtension
    {
        public static string ToExceptionString(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry entry)
        {
            if (entry == null) return string.Empty;
            var pi = entry.GetType().GetProperty("SubKey");
            if (pi == null) return string.Empty;
            return $"{pi.GetValue(entry, null)}".Trim();
        }
    }
}
