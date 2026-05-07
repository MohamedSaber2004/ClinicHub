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

            builder.HasMany(x => x.Messages)
                .WithOne()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(x => x.InitiatorId)
                .IsRequired();

            builder.Property(x => x.RecipientId)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("Conversations");
        }
    }
}
