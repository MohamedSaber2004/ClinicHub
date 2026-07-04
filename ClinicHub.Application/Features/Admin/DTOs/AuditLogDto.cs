namespace ClinicHub.Application.Features.Admin.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? UserName { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
