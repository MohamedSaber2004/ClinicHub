using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Subscriptions.DTOs
{
    public class InitiateSubscriptionPaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public string PaymobRedirectUrl { get; set; } = null!;
        public string RedirectUrl { get; set; } = null!;
        public string PaymentUrl { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string PaymobPaymentKey { get; set; } = null!;
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; }
        public SubscriptionPlan Period { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
    }
}
