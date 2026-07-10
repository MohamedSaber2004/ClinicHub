using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class UserVerificationConfiguration : IEntityTypeConfiguration<UserVerification>
{
    public void Configure(EntityTypeBuilder<UserVerification> builder)
    {
        builder.ToTable("UserVerifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.RequestedRole).IsRequired().HasConversion<int>();
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ProfessionalPracticeCardImage).HasMaxLength(500);
        builder.Property(x => x.TaxCardImage).HasMaxLength(500);
        builder.Property(x => x.UnionIdCardImage).HasMaxLength(500);
        builder.Property(x => x.DoctorImage).HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
