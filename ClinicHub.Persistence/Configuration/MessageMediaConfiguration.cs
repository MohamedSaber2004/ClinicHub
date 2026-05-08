using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class MessageMediaConfiguration : IEntityTypeConfiguration<MessageMedia>
    {
        public void Configure(EntityTypeBuilder<MessageMedia> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageId).IsRequired();
            builder.Property(x => x.MediaType).HasConversion<int>().IsRequired();
            builder.Property(x => x.FileName).HasMaxLength(255);

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("MessageMedia");
        }
    }
}
