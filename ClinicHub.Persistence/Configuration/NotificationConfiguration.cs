using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TitleEn)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.TitleAr)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.BodyEn)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.BodyAr)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.ClinicId);
        }
    }
}
