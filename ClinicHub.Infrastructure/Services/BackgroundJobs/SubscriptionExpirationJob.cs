using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class SubscriptionExpirationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpirationJob> _logger;

    public SubscriptionExpirationJob(IServiceProvider serviceProvider, ILogger<SubscriptionExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExpireAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var subscription = await unitOfWork.GetRepository<Subscription, Guid>().FindByKeyAsync(subscriptionId, cancellationToken);
        if (subscription == null || subscription.Status != SubscriptionStatus.Active)
            return;

        if (subscription.EndDate > DateTime.UtcNow)
            return;

        subscription.Status = SubscriptionStatus.Expired;
        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Subscription {SubscriptionId} marked as expired.", subscriptionId);
    }

    public async Task SweepExpiredAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredSubscriptions = await unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.Status = SubscriptionStatus.Expired;
        }

        if (expiredSubscriptions.Count > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} subscriptions", expiredSubscriptions.Count);
        }
    }
}
