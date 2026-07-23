using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Plan : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int? MaxDoctors { get; set; }
        public int? MaxStaff { get; set; }
        public string? Features { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public ICollection<PlanPermission> Permissions { get; set; } = new List<PlanPermission>();
    }
}
