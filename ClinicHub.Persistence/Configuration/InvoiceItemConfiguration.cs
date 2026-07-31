using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.Discount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxRate).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).IsRequired().HasColumnType("decimal(18,2)");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => x.InvoiceId);
    }
}
