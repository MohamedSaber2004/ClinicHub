using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("Advertisements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.TargetUrl).HasMaxLength(500);
        builder.Property(x => x.DurationDays).IsRequired();
        builder.Property(x => x.AmountPaid).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();

        builder.HasOne(x => x.Clinic)
            .WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AdPackage)
            .WithMany()
            .HasForeignKey(x => x.AdPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Payment)
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ClinicId);
        builder.HasIndex(x => new { x.ClinicId, x.Status });
        builder.HasIndex(x => x.PaymentId).IsUnique().HasFilter("\"PaymentId\" IS NOT NULL");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
