using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class UserClinicConfiguration : IEntityTypeConfiguration<UserClinic>
    {
        public void Configure(EntityTypeBuilder<UserClinic> builder)
        {
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserId, x.ClinicId }).IsUnique();
        }
    }
}
