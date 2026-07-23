using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DescriptionAr).HasMaxLength(1000);
        builder.Property(x => x.PriceMonthly).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.PriceYearly).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.Features).HasMaxLength(2000);
        builder.Property(x => x.SortOrder).IsRequired();

        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SortOrder);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
