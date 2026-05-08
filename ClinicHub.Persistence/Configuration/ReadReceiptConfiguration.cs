using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class ReadReceiptConfiguration : IEntityTypeConfiguration<ReadReceipt>
    {
        public void Configure(EntityTypeBuilder<ReadReceipt> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ReadAt).IsRequired();

            builder.HasIndex(x => new { x.MessageId, x.UserId }).IsUnique();

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("ReadReceipts");
        }
    }
}
