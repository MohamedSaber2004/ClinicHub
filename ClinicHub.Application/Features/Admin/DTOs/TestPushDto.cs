using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Admin.DTOs
{
    public class TestPushResultDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TotalDevices { get; set; }
        public int DeliveredCount { get; set; }
        public IList<TestPushDeviceDto> Devices { get; set; } = new List<TestPushDeviceDto>();
    }

    public class TestPushDeviceDto
    {
        public DevicePlatform Platform { get; set; }
        public string TokenPreview { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorCode { get; set; }
        public string? Error { get; set; }
    }
}
