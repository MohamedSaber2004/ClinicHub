using ClinicHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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
        DbSet<Doctor> Doctors { get; }
        DbSet<DoctorAvailability> DoctorAvailabilities { get; }
        DbSet<Appointment> Appointments { get; }
        DbSet<UserFbToken> UserFbTokens { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<UserRefreshToken> UserRefreshTokens { get; }
        DbSet<SupportTicket> SupportTickets { get; }
        DbSet<Subscription> Subscriptions { get; }
        DbSet<Advertisement> Advertisements { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<IdentityUserRole<Guid>> UserRoles { get; }
        DbSet<IdentityRole<Guid>> Roles { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
