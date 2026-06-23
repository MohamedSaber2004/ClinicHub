namespace ClinicHub.Application.Features.Booking.BookingConfig.DTOs
{
    public class BookingConfigResponseDto
    {
        public decimal ConsultationFee { get; set; }
        public string Currency { get; set; } = null!;
        public int SlotDurationMinutes { get; set; }
        public int MaxFutureDays { get; set; }
        public int ReservationTtlMinutes { get; set; }
        public List<string> PaymentMethods { get; set; } = new();
        public bool AllowOnlineBooking { get; set; }
        public bool RequirePayment { get; set; }
    }
}
