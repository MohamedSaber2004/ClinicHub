using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class PayoutBatchConfiguration : IEntityTypeConfiguration<PayoutBatch>
{
    public void Configure(EntityTypeBuilder<PayoutBatch> builder)
    {
        builder.ToTable("PayoutBatches");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(40);
        builder.Property(b => b.Status).IsRequired().HasConversion<int>();
        builder.Property(b => b.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(b => b.BudgetBefore).HasColumnType("decimal(18,2)");
        builder.Property(b => b.BudgetAfter).HasColumnType("decimal(18,2)");
        builder.Property(b => b.FailureNotes).HasMaxLength(2000);

        builder.HasIndex(b => b.Code).IsUnique();
        builder.HasIndex(b => b.Status);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Batch)
            .HasForeignKey(i => i.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(PayoutBatch.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
