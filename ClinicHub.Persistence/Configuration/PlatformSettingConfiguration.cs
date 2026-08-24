using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
    {
        public void Configure(EntityTypeBuilder<PlatformSetting> builder)
        {
            builder.ToTable("PlatformSettings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AppointmentFeePercent)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0m);
        }
    }
}
