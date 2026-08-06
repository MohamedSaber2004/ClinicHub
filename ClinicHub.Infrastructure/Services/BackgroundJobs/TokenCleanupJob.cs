using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class TokenCleanupJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(IServiceProvider serviceProvider, ILogger<TokenCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var now = DateTime.Now;

        var expiredTokens = await unitOfWork.GetRepository<UserRefreshToken, Guid>()
            .GetAllAsync(t => !t.IsRevoked && t.ExpiryDate < now)
            .ToListAsync(cancellationToken);

        foreach (var token in expiredTokens)
        {
            token.Revoke();
        }

        var users = await userManager.Users
            .Where(u => (u.PasswordResetToken != null && u.PasswordResetTokenExpiry < now)
                     || (u.VerificationCode != null && u.VerificationCodeExpiry < now))
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            if (user.PasswordResetToken != null && user.PasswordResetTokenExpiry < now)
                user.ClearPasswordResetToken();

            if (user.VerificationCode != null && user.VerificationCodeExpiry < now)
                user.ClearVerificationCode();

            await userManager.UpdateAsync(user);
        }

        if (expiredTokens.Count > 0 || users.Count > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Token cleanup: revoked {TokenCount} refresh tokens, cleared {UserCount} expired user tokens.", expiredTokens.Count, users.Count);
        }
    }
}
