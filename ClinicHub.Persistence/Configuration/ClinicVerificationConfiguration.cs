using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class ClinicVerificationConfiguration : IEntityTypeConfiguration<ClinicVerification>
{
    public void Configure(EntityTypeBuilder<ClinicVerification> builder)
    {
        builder.ToTable("ClinicVerifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Clinic)
            .WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClinicId);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
