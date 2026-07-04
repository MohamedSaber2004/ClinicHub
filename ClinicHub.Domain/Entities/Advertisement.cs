using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Advertisement : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid? ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
        public string Title { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Active;
        public decimal AmountPaid { get; set; }
    }
}
