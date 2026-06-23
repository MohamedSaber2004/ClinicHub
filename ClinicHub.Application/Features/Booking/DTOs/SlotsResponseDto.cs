namespace ClinicHub.Application.Features.Booking.DTOs
{
    public class SlotsResponseDto
    {
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        public string Date { get; set; } = null!;
        public int SlotDurationMinutes { get; set; }
        public WorkingHoursDto? WorkingHours { get; set; }
        public List<AvailableSlotDto> Slots { get; set; } = new();
    }

    public class WorkingHoursDto
    {
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
    }
}
