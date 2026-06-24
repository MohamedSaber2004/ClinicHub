namespace ClinicHub.Application.Features.Booking.BookingConfig.DTOs
{
    public class UpdateBookingConfigDto
    {
        public decimal ConsultationFee { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public int MaxAdvanceBookingDays { get; set; } = 30;
        public int ReservationTtlMinutes { get; set; } = 10;
    }
}
