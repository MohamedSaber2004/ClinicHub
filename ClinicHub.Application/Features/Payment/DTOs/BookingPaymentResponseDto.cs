using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Payment.DTOs
{
    public class BookingPaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public Guid? ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? RedirectUrl { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ReceiptUrl { get; set; }
    }
}
