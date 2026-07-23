using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Subscription : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid ClinicId { get; set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic Clinic { get; set; } = null!;
        public Guid? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public SubscriptionPlan Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public Guid? PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public string? Notes { get; set; }
    }
}
