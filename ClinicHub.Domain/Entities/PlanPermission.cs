using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class PlanPermission : BaseEntity<Guid>
    {
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = null!;
        public SubscriptionPermission Permission { get; set; }
    }
}
