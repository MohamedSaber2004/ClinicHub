using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IClinicHubContext: IAsyncDisposable
    {
        DbSet<Post> Posts { get; }
        DbSet<Comment> Comments { get; }
        DbSet<Reaction> Reactions { get; }
        DbSet<Media> Media { get; }
        DbSet<Clinic> Clinics { get; }
        DbSet<Specialization> Specializations { get; }
        DbSet<UserFbToken> UserFbTokens { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<UserRefreshToken> UserRefreshTokens { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
