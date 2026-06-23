namespace ClinicHub.Application.Features.Booking.DTOs
{
    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
