namespace ClinicHub.Application.Features.Availability.DTOs
{
    public class TimeSlotDto
    {
        public Guid Id { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public bool IsAvailable { get; set; }
    }
}

