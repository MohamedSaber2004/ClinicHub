namespace ClinicHub.Application.Common.Models
{
    public class PushSendReport
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorCode { get; set; }
        public string? Error { get; set; }
    }
}
