using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.ToTable("Clinics");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.ArDescription)
                .HasMaxLength(2000);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.AddressAr)
                .HasMaxLength(500);

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .HasMaxLength(200);

            builder.Property(x => x.Website)
                .HasMaxLength(500);

            builder.Property(x => x.Logo)
                .HasMaxLength(1000);

            builder.Property(x => x.WorkingHours)
                .HasMaxLength(1000);

            builder.Property(x => x.WorkingHoursStart)
                .HasColumnType("time");

            builder.Property(x => x.WorkingHoursEnd)
                .HasColumnType("time");

            builder.Property(x => x.WorkingDays)
                .HasMaxLength(200);

            builder.Property(x => x.Location)
                .HasColumnType("geography");

            builder.HasIndex(x => x.Location)
                .HasDatabaseName("IX_Clinics_Location")
                .HasAnnotation("SqlServer:SpatialIndex", true);

            builder.Property(x => x.IsRegistered)
                .HasDefaultValue(true);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ClinicStatus.Active);

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasOne(x => x.Specialization)
                .WithMany(x => x.Clinics)
                .HasForeignKey(x => x.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClinicAdmin)
                .WithMany()
                .HasForeignKey(x => x.ClinicAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
