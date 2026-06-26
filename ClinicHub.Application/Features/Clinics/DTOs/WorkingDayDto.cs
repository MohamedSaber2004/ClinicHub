namespace ClinicHub.Application.Features.Clinics.DTOs
{
    public class WorkingDayDto
    {
        public string DayOfWeek { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
