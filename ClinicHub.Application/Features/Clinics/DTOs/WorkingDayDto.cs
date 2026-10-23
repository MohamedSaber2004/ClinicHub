namespace ClinicHub.Application.Features.Clinics.DTOs
{
    public class WorkingDayDto
    {
        public string DayOfWeek { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public static string ToDayLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value?.Trim() ?? string.Empty;

            var trimmed = value.Trim();
            return Enum.TryParse<DayOfWeek>(trimmed, ignoreCase: true, out var parsed)
                ? parsed.ToString()
                : trimmed;
        }
    }
}
