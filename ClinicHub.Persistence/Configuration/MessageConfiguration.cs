using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConversationId)
                .IsRequired();

            builder.Property(x => x.SenderId)
                .IsRequired();

            builder.Property(x => x.Content)
                .HasMaxLength(5000)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(ClinicHub.Domain.Enums.MessageStatus.Pending); // Pending

            builder.Property(x => x.IsEdited)
                .HasDefaultValue(false);

            builder.Property(x => x.Version)
                .IsRowVersion();

            // Relationships
            builder.HasOne(x => x.ReplyToMessage)
                .WithMany()
                .HasForeignKey(x => x.ReplyToMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Reactions)
                .WithOne(r => r.Message)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Media)
                .WithOne(m => m.Message)
                .HasForeignKey(m => m.MessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.ToTable("Messages");
        }
    }
}

