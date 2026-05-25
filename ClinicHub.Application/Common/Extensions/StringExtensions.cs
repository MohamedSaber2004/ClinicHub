using System.Text;

namespace ClinicHub.Application.Common.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeArabic(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var result = text.Trim();

            // Normalize Alif
            result = result.Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا');

            // Normalize Taa Marbuta
            result = result.Replace('ة', 'ه');

            // Normalize Ya
            result = result.Replace('ى', 'ي');

            return result;
        }

        public static bool ContainsArabic(this string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return false;
            return source.NormalizeArabic().Contains(target.NormalizeArabic(), StringComparison.OrdinalIgnoreCase);
        }

        public static string ToPaymobFormat(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return phoneNumber;

            var cleaned = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            // If it starts with 20 and has 12 digits (Egypt international format), convert to local 11-digit
            if (cleaned.StartsWith("20") && cleaned.Length == 12)
                return "0" + cleaned.Substring(2);

            // If it starts with 01 and has 11 digits (Egypt local format), keep it as is
            if (cleaned.StartsWith("01") && cleaned.Length == 11)
                return cleaned;

            // If it starts with 1 and has 10 digits (Egypt local without leading 0), add leading 0
            if (cleaned.StartsWith("1") && cleaned.Length == 10)
                return "0" + cleaned;

            // Fallback: if it starts with 01, return as is (local format)
            if (cleaned.StartsWith("01"))
                return cleaned;

            return cleaned;
        }
    }
}
