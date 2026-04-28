using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;
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
        public ClinicHubContext(ICurrentUserService? currentUserService, DbContextOptions<ClinicHubContext> options)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<UserFbToken> UserFbTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }


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
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.MarkAsCreated(_currentUserService?.UserId.ToString() ?? "System");
                        break;
                    case EntityState.Modified:
                        entry.Entity.MarkAsUpdated(_currentUserService?.UserId.ToString() ?? "System");
                        break;
                    case EntityState.Deleted:
                        // Only convert to Soft Delete if the entity is already persisted (not a new entity being detached)
                        entry.State = EntityState.Modified;
                        entry.Entity.MarkAsDeleted(_currentUserService?.UserId.ToString() ?? "System");
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
