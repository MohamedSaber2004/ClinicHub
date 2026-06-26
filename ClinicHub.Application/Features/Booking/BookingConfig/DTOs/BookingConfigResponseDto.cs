namespace ClinicHub.Application.Features.Booking.BookingConfig.DTOs
{
    public class BookingConfigResponseDto
    {
        public decimal ConsultationFee { get; set; }
        public string Currency { get; set; } = null!;
        public int MaxAdvanceBookingDays { get; set; }
        public int ReservationTtlMinutes { get; set; }
    }
}
