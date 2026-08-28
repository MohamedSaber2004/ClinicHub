using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class ClinicAdSettingsConfiguration : IEntityTypeConfiguration<ClinicAdSettings>
{
    public void Configure(EntityTypeBuilder<ClinicAdSettings> builder)
    {
        builder.ToTable("ClinicAdSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ClinicId).IsUnique();
        builder.Property(x => x.MaxAds).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.MaxImpressions).IsRequired().HasDefaultValue(0);
        builder.HasOne(x => x.Clinic).WithMany().HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Cascade);
    }
}
