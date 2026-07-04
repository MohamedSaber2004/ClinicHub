using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;

namespace ClinicHub.Domain.Entities
{
    public class UserClinic : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;

        public Guid ClinicId { get; private set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic Clinic { get; private set; } = null!;

        public DateTime FollowedAt { get; private set; }

        private UserClinic() { }

        public UserClinic(Guid userId, Guid clinicId)
        {
            UserId = userId;
            ClinicId = clinicId;
            FollowedAt = DateTime.UtcNow;
        }
    }
}
