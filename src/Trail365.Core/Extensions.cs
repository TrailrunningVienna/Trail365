using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Trail365.Entities;

namespace Trail365
{
    public static class Extensions
    {
        public static string GetEnumDescription(Enum value)
        {
            var enumType = value.GetType();
            FieldInfo fi = enumType.GetField(value.ToString());
            if (fi == null)
            {
                throw new InvalidOperationException($"Value '{value}' not defined on Enum type '{enumType.FullName}'");
            }

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        public static string ToKmFormat(this double value)
        {
            //km...one digit for decimals
            string v = value.ToString("### ### ### ##0.0");
            return v;
        }

        public static string ToCurrencyFormat(this decimal value)
        {
            string v = value.ToString("### ### ### ##0.00", new CultureInfo("de-AT")).Trim();
            return v.Replace(" ", ".");
        }

        public static string ToCurrencyFormatShort(this decimal value)
        {
            string v = value.ToString("### ### ### ##0", new CultureInfo("de-AT")).Trim();
            return v.Replace(" ", ".");
        }

        public static string ToDateFormatForDefault(this DateTime? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }
            return value.Value.ToString("dd.MM.yyyy");
        }

        public static string ToIntFormat(this int value)
        {
            string v = value.ToString("### ### ### ##0", new CultureInfo("de-AT")).Trim();
            return v.Replace(" ", ".");
        }

        public static string ToCurrencyFormat(this decimal? value)
        {
            return value.HasValue ? value.Value.ToCurrencyFormat() : string.Empty;
        }

        public static string ToCurrencyFormatShort(this decimal? value)
        {
            return value.HasValue ? value.Value.ToCurrencyFormatShort() : string.Empty;
        }

        public static string ToIntFormat(this int? value)
        {
            return value.GetValueOrDefault().ToIntFormat();
        }

        public static string GenerateAnzeigeID()
        {
            var rnd = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                sb.Append((char)rnd.Next('A', 'Z'));
            }
            for (int i = 0; i < 9; i++)
            {
                sb.Append((char)rnd.Next('0', '9'));
            }

            return sb.ToString();
        }

        public static string ToDescription<TEnum>(this TEnum EnumValue) where TEnum : struct
        {
            return GetEnumDescription((Enum)(object)((TEnum)EnumValue));
        }

        public static AuthenticationType ConvertAuthenticationType(this string authenticationType)
        {
            if (string.IsNullOrEmpty(authenticationType)) throw new ArgumentNullException(nameof(authenticationType));
            switch (authenticationType)
            {
                case "Google":
                    return AuthenticationType.Google;

                case "Facebook":
                    return AuthenticationType.Facebook;

                default:
                    return AuthenticationType.Cookies;
            }
        }

        public static bool TryGetUserIdClaim(this ClaimsPrincipal principal, out Claim id)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            id = principal.FindFirst(ClaimTypes.NameIdentifier);
            return (id != null);
        }

        public static bool TryGetMailClaim(this ClaimsPrincipal principal, out Claim id)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            id = principal.FindFirst(ClaimTypes.Email);
            return (id != null);
        }

        public static bool IsSuperAdmin(this ClaimsPrincipal principal, string superUsers)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (string.IsNullOrEmpty(superUsers)) return false;
            if (TryGetMailClaim(principal, out var email))
            {
                string match = $"{email.Value}".ToLowerInvariant().Trim();
                if (string.IsNullOrEmpty(match)) return false;
                string[] users = superUsers.ToLowerInvariant().Split(UserSeparators, StringSplitOptions.RemoveEmptyEntries);
                return users.Contains(match);
            }
            return false;
        }

        private readonly static string[] UserSeparators = new string[] { ";", ",", "|" };
    }
}
