using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Admin.DTOs
{
    public class SupportTicketDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public SupportTicketStatus Status { get; set; }
        public SupportTicketPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
