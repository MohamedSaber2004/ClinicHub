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

        /// <summary>When the subscription was cancelled (null while active).</summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>Who cancelled it: the clinic owner, an admin, or the system.</summary>
        public SubscriptionCancellationSource? CancellationSource { get; set; }

        /// <summary>User who performed the cancellation (owner or admin).</summary>
        public Guid? CancelledByUserId { get; set; }

        /// <summary>Human-readable cancellation reason.</summary>
        public string? CancellationReason { get; set; }

        /// <summary>Pro-rated refund amount issued (or to be issued) for this subscription.</summary>
        public decimal RefundAmount { get; set; }
    }
}
