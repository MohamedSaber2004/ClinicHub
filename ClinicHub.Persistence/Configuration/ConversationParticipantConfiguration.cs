using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
    {
        public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConversationId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.IsMuted).HasDefaultValue(false);
            builder.Property(x => x.IsArchived).HasDefaultValue(false);
            builder.Property(x => x.IsBlocked).HasDefaultValue(false);
            builder.Property(x => x.JoinedAt).IsRequired();

            builder.HasIndex(x => new { x.ConversationId, x.UserId }).IsUnique();

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("ConversationParticipants");
        }
    }
}
