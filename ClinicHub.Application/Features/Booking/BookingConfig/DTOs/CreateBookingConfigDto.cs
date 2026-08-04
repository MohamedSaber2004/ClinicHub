namespace ClinicHub.Application.Features.Booking.BookingConfig.DTOs
{
    public class CreateBookingConfigDto
    {
        public decimal ConsultationFee { get; set; }
        public int MaxAdvanceBookingDays { get; set; } = 30;
        public int ReservationTtlMinutes { get; set; } = 10;
        public int CancellationWindowMinutes { get; set; } = 120;
    }
}
