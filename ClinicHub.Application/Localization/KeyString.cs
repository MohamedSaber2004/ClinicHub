namespace ClinicHub.Application.Localization
{
    public class KeyString
    {
        private readonly string key;

        public KeyString(string value)
        {
            this.key = value;
        }

        public string Value
        {
            get
            {
                return JsonLocalizationProvider.GetLocalizedString(this.key);
            }
        }

        public string GetLocalizedValue()
        {
            return this.Value;
        }

        public string Key
        {
            get
            {
                return this.key;
            }
        }

        public static implicit operator string(KeyString keyString)
        {
            return keyString?.Value ?? string.Empty;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
