using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.ToTable("Ratings");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Value)
                .IsRequired();

            builder.Property(r => r.Review)
                .HasMaxLength(1000);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.Doctor)
                .WithMany()
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.Clinic)
                .WithMany()
                .HasForeignKey(r => r.ClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(r => new { r.UserId, r.DoctorId })
                .IsUnique()
                .HasFilter("[DoctorId] IS NOT NULL");

            builder.HasIndex(r => new { r.UserId, r.ClinicId })
                .IsUnique()
                .HasFilter("[ClinicId] IS NOT NULL");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
