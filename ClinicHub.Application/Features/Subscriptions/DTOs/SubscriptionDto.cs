using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Subscriptions.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public SubscriptionPlan Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsActive => Status == SubscriptionStatus.Active && EndDate > DateTime.UtcNow;
    }
}
