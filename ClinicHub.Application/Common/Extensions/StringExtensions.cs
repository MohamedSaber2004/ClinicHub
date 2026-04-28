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
    }
}
