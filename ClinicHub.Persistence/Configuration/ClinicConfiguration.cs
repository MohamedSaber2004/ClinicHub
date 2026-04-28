using ClinicHub.Domain.Entities;
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
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.AddressAr)
                .HasMaxLength(500);

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.Location)
                .IsRequired()
                .HasColumnType("geography");

            builder.HasIndex(x => x.Location)
                .HasDatabaseName("IX_Clinics_Location")
                .HasAnnotation("SqlServer:SpatialIndex", true);

            builder.Property(x => x.IsRegistered)
                .HasDefaultValue(true);

            builder.HasOne(x => x.Specialization)
                .WithMany(x => x.Clinics)
                .HasForeignKey(x => x.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
