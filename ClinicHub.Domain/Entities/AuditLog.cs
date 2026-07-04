using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;

namespace ClinicHub.Domain.Entities
{
    public class AuditLog : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid? ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Action { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
