using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Advertisements.DTOs
{
    public class AdvertisementDto
    {
        public Guid Id { get; set; }
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public string Title { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AdvertisementStatus Status { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
