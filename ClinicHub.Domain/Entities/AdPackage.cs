using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public class AdPackage : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
