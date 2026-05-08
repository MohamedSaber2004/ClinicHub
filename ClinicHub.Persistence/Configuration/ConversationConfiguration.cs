using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(255);

            builder.Property(x => x.IsGroup)
                .HasDefaultValue(false);

            builder.Property(x => x.Version)
                .IsRowVersion();

            // Relationships
            builder.HasMany(x => x.Messages)
                .WithOne()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Participants)
                .WithOne(p => p.Conversation)
                .HasForeignKey(p => p.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("Conversations");
        }
    }
}
