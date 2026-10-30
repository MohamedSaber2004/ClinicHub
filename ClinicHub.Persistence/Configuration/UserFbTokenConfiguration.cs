using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class UserFbTokenConfiguration : IEntityTypeConfiguration<UserFbToken>
    {
        public void Configure(EntityTypeBuilder<UserFbToken> builder)
        {
            builder.ToTable("UserFbTokens");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.DevicePlatform)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserFbTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Token)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}
