using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.Property(p => p.Status).IsRequired().HasConversion<int>();
        builder.Property(p => p.PaymentMethod).HasMaxLength(50);
        builder.Property(p => p.RedirectUrl).HasMaxLength(500);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.TransactionId).HasMaxLength(100);
        builder.Property(p => p.RefundReason).HasMaxLength(500);
        builder.HasIndex(p => p.AppointmentId);

        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Clinic)
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ClinicId);
        builder.HasIndex(p => p.SubscriptionId).IsUnique().HasFilter("[SubscriptionId] IS NOT NULL");
    }
}