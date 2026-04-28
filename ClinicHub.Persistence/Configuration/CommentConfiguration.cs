using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Reactions)
                .WithOne(x => x.Comment)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Restrict)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(x => x.ParentComment)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction)
                .Metadata.DependentToPrincipal!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Media)
                .WithOne(x => x.Comment)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Restrict)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(x => x.Content)
                .HasMaxLength(2000)
                .IsRequired();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("Comments");
        }
    }
}
