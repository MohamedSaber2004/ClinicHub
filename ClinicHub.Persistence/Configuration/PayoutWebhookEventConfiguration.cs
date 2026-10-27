using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class PayoutWebhookEventConfiguration : IEntityTypeConfiguration<PayoutWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PayoutWebhookEvent> builder)
    {
        builder.ToTable("PayoutWebhookEvents");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.PaymobTransactionId).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Issuer).HasMaxLength(30);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.StatusCode).HasMaxLength(20);
        builder.Property(e => e.DisbursementStatus).HasMaxLength(40);
        builder.Property(e => e.Error).HasMaxLength(1000);

        // A replayed callback must be detectable, hence one row per transaction id.
        builder.HasIndex(e => e.PaymobTransactionId).IsUnique();
        builder.HasIndex(e => e.ProcessedAt);
    }
}
