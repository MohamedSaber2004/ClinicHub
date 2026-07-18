using System.Reflection;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClinicHub.Persistence
{
    public class ClinicHubContext: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IClinicHubContext
    {
        private readonly ICurrentUserService? _currentUserService;
        private readonly Guid? _currentClinicId;
        public ClinicHubContext(ICurrentUserService? currentUserService, DbContextOptions<ClinicHubContext> options)
            : base(options)
        {
            _currentUserService = currentUserService;
            _currentClinicId = _currentUserService?.CurrentClinicId;
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<UserFbToken> UserFbTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReaction> MessageReactions { get; set; }
        public DbSet<MessageMedia> MessageMedia { get; set; }
        public DbSet<ReadReceipt> ReadReceipts { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<BookingConfiguration> BookingConfigurations { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.ConfigureWarnings(action =>
            {
                action.Ignore(CoreEventId.InvalidIncludePathError);
            });
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ClinicHubContext).Assembly,
                type => type.Namespace is not null && type.Namespace.EndsWith("Configuration"));

            builder.HasDefaultSchema("dbo");

            foreach (var entityType in builder.Model.GetEntityTypes()
                .Where(e => typeof(IClinicScopedEntity).IsAssignableFrom(e.ClrType)))
            {
                var method = typeof(ClinicHubContext)
                    .GetMethod(nameof(ApplyClinicFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(this, new object[] { builder });
            }
        }

        private void ApplyClinicFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity, IClinicScopedEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e =>
                !e.IsDeleted && (_currentClinicId == null || e.ClinicId == null || e.ClinicId == _currentClinicId));
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService?.UserId.ToString() ?? "System";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.MarkAsCreated(userId);
                        break;
                    case EntityState.Modified:
                        entry.Entity.MarkAsUpdated(userId);
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.MarkAsDeleted(userId);
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is ApplicationUser && e.State != EntityState.Detached))
            {
                var user = (ApplicationUser)entry.Entity;
                switch (entry.State)
                {
                    case EntityState.Added:
                        user.CreatedAt = DateTime.Now;
                        user.CreatedBy = userId;
                        user.IsActive = true;
                        break;
                    case EntityState.Modified:
                        user.UpdatedAt = DateTime.Now;
                        user.UpdatedBy = userId;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        user.IsDeleted = true;
                        user.DeletedAt = DateTime.Now;
                        user.DeletedBy = userId;
                        user.IsActive = false;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
