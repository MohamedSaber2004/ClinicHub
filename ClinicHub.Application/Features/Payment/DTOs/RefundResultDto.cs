namespace ClinicHub.Application.Features.Payment.DTOs
{
    public class RefundResultDto
    {
        public bool Success { get; set; }
        public string? RefundId { get; set; }
        public string? Message { get; set; }
    }
}
