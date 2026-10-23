using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Subscriptions.DTOs
{
    /// <summary>
    /// Result of a successful cancellation. The cancellation always succeeds once
    /// this is returned; <see cref="RefundStatus"/> only describes the money.
    /// </summary>
    public class CancelSubscriptionResponseDto
    {
        public Guid SubscriptionId { get; set; }
        public DateTime CancelledAt { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public decimal ChargedAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentRefundStatus RefundStatus { get; set; } = PaymentRefundStatus.None;
        public string? RefundId { get; set; }
    }
}
