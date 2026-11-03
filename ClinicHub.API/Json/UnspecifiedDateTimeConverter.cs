using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClinicHub.API.Json
{
    /// <summary>
    /// Timezone-free DateTime converter. Takes the calendar date exactly as written
    /// by the client (yyyy-MM-dd) and ignores any time/offset suffix, so
    /// "2026-09-13", "2026-09-13T00:00:00" and "2026-09-13T00:00:00+03:00" all bind
    /// to the same Unspecified 2026-09-13 midnight. Nothing is converted to
    /// server-local or UTC — the day can never shift to Saturday.
    /// </summary>
    public sealed class UnspecifiedDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString()?.Trim() ?? string.Empty;
                // Calendar date is everything before 'T' or space — offset is dropped.
                var datePart = raw.Split(['T', ' '], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? raw;
                if (DateTime.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
                if (DateTime.TryParse(datePart, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
                    return new DateTime(fallback.Year, fallback.Month, fallback.Day, 0, 0, 0, DateTimeKind.Unspecified);
                throw new JsonException($"Invalid date value: '{raw}'. Expected yyyy-MM-dd.");
            }
            if (reader.TokenType == JsonTokenType.Null)
                throw new JsonException("Cannot convert null to DateTime.");
            throw new JsonException($"Unexpected token {reader.TokenType} for DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Unspecified)
                .ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
    }

    public sealed class UnspecifiedNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString()?.Trim();
                if (string.IsNullOrEmpty(raw))
                    return null;
                var datePart = raw.Split(['T', ' '], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? raw;
                if (DateTime.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
                if (DateTime.TryParse(datePart, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
                    return new DateTime(fallback.Year, fallback.Month, fallback.Day, 0, 0, 0, DateTimeKind.Unspecified);
                throw new JsonException($"Invalid date value: '{raw}'. Expected yyyy-MM-dd.");
            }
            throw new JsonException($"Unexpected token {reader.TokenType} for DateTime?.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 0, 0, 0,
                    DateTimeKind.Unspecified).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}
