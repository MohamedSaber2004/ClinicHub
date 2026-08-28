using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class ClinicAdSettings : BaseEntity<Guid>
    {
        public Guid ClinicId { get; set; }
        public Clinic Clinic { get; set; } = null!;
        public int MaxAds { get; set; } = 0;
        public int MaxImpressions { get; set; } = 0;
    }
}
