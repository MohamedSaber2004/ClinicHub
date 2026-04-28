using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.Property(u => u.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.ProfilePictureUrl)
                .HasMaxLength(500);

            builder.Property(u => u.FacebookUserId)
                .HasMaxLength(200);

            builder.Property(u => u.GoogleUserId)
                .HasMaxLength(200);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(u => u.BirthDate)
                .IsRequired();

            builder.Property(u => u.PasswordResetToken)
                .HasMaxLength(2000);

            builder.Property(u => u.Language)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);
        }
    }
}
