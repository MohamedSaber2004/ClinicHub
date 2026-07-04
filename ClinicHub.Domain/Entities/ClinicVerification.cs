using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class ClinicVerification : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid ClinicId { get; set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic Clinic { get; set; } = null!;
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public ApplicationUser? ReviewedBy { get; set; }
        public string? Notes { get; set; }
    }
}
