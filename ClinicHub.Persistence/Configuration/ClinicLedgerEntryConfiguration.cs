using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class ClinicLedgerEntryConfiguration : IEntityTypeConfiguration<ClinicLedgerEntry>
{
    public void Configure(EntityTypeBuilder<ClinicLedgerEntry> builder)
    {
        builder.ToTable("ClinicLedgerEntries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).IsRequired().HasConversion<int>();
        builder.Property(e => e.Status).IsRequired().HasConversion<int>();
        builder.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(e => e.Currency).IsRequired().HasMaxLength(3);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => new { e.ClinicId, e.Status, e.AvailableAt });

        // One earning per payment — the database guarantee that a payment is never accrued twice.
        builder.HasIndex(e => e.PaymentId)
            .IsUnique()
            .HasFilter("[PaymentId] IS NOT NULL AND [Type] = 0");

        // Reserved earnings point at their payout item; one payout row per item keeps
        // settled money from being written twice for the same transfer.
        builder.HasIndex(e => e.PayoutItemId);
        builder.HasIndex(e => e.PayoutItemId)
            .IsUnique()
            .HasFilter("[PayoutItemId] IS NOT NULL AND [Type] = 2");

        builder.HasOne(e => e.Payment)
            .WithMany()
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
