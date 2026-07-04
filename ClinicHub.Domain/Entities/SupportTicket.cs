using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class SupportTicket : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public Guid? ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;
        public DateTime? ResolvedAt { get; set; }
    }
}
