using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class ClinicPayoutAccountConfiguration : IEntityTypeConfiguration<ClinicPayoutAccount>
{
    public void Configure(EntityTypeBuilder<ClinicPayoutAccount> builder)
    {
        builder.ToTable("ClinicPayoutAccounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.HolderName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.BankCode).IsRequired().HasMaxLength(20);
        builder.Property(a => a.AccountNumberEncrypted).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.AccountLast4).IsRequired().HasMaxLength(4);
        builder.Property(a => a.NationalIdEncrypted).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.NationalIdLast4).IsRequired().HasMaxLength(4);
        builder.Property(a => a.BankTransactionType).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Status).IsRequired().HasConversion<int>();
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        builder.HasIndex(a => a.ClinicId).IsUnique();

        builder.HasOne(a => a.Clinic)
            .WithMany()
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
