using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("Media");
        }
    }
}
