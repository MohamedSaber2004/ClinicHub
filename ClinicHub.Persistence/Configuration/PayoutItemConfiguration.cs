using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class PayoutItemConfiguration : IEntityTypeConfiguration<PayoutItem>
{
    public void Configure(EntityTypeBuilder<PayoutItem> builder)
    {
        builder.ToTable("PayoutItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
        builder.Property(i => i.Status).IsRequired().HasConversion<int>();
        builder.Property(i => i.PaymobTransactionId).HasMaxLength(50);
        builder.Property(i => i.PaymobStatusCode).HasMaxLength(20);
        builder.Property(i => i.DisbursementStatusRaw).HasMaxLength(40);
        builder.Property(i => i.LastError).HasMaxLength(1000);

        builder.HasIndex(i => i.BatchId);
        builder.HasIndex(i => new { i.ClinicId, i.Status });

        // Unique so a retry that races with an ambiguous first attempt cannot fork a
        // second payment for the same clinic.
        builder.HasIndex(i => i.PaymobTransactionId)
            .IsUnique()
            .HasFilter("[PaymobTransactionId] IS NOT NULL");

        builder.HasOne(i => i.Clinic)
            .WithMany()
            .HasForeignKey(i => i.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PayoutAccount)
            .WithMany()
            .HasForeignKey(i => i.PayoutAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
