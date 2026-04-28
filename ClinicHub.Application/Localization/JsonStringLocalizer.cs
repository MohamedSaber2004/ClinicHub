using Microsoft.Extensions.Localization;
using System.Globalization;

namespace ClinicHub.Application.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly string? _culture;

        public JsonStringLocalizer(string? culture = null)
        {
            _culture = culture;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = JsonLocalizationProvider.GetLocalizedString(name, _culture);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = JsonLocalizationProvider.GetLocalizedString(name, _culture, arguments);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            // This is optional but good for completeness
            return Enumerable.Empty<LocalizedString>();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new JsonStringLocalizer(culture.Name);
        }
    }
}
