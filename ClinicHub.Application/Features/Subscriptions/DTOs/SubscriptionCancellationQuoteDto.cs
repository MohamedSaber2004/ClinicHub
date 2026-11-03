using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Subscriptions.DTOs
{
    /// <summary>
    /// Pro-rated cancellation preview. Returned by GET /subscriptions/my/cancellation-quote.
    /// </summary>
    public class SubscriptionCancellationQuoteDto
    {
        public Guid SubscriptionId { get; set; }
        public bool Eligible { get; set; }
        public string? Reason { get; set; }
        public string? PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentRefundStatus RefundStatus { get; set; } = PaymentRefundStatus.None;
    }
}
