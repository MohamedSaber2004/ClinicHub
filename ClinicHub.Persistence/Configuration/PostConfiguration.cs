using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Restrict)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Reactions)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Restrict)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Media)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Restrict)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(x => x.Content)
                .HasMaxLength(2000)
                .IsRequired();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("Posts");
        }
    }
}
