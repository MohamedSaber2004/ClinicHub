using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Specialization : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string ArName { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public bool IsFamous { get; set; }

        public ICollection<Clinic> Clinics { get; set; } = new HashSet<Clinic>();
    }
}
