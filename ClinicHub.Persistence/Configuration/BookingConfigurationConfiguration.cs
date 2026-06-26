using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class BookingConfigurationConfiguration : IEntityTypeConfiguration<BookingConfiguration>
    {
        public void Configure(EntityTypeBuilder<BookingConfiguration> builder)
        {
            builder.ToTable("BookingConfigurations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConsultationFee)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("EGP");

            builder.Property(x => x.MaxAdvanceBookingDays)
                .IsRequired()
                .HasDefaultValue(30);

            builder.Property(x => x.ReservationTtlMinutes)
                .IsRequired()
                .HasDefaultValue(10);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ClinicId)
                .IsUnique();

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
