using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.InvoiceNumber).HasMaxLength(50);
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.SubTotal).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountValue).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountType).IsRequired().HasConversion<int>();
        builder.Property(x => x.TaxRate).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.CancellationReason).HasMaxLength(500);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ClinicId, x.InvoiceNumber }).IsUnique();
        builder.HasIndex(x => new { x.ClinicId, x.Status });
        builder.HasIndex(x => x.ClinicId);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
