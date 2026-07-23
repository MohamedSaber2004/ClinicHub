using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class PlanPermissionConfiguration : IEntityTypeConfiguration<PlanPermission>
{
    public void Configure(EntityTypeBuilder<PlanPermission> builder)
    {
        builder.ToTable("PlanPermissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Permission)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PlanId, x.Permission }).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
