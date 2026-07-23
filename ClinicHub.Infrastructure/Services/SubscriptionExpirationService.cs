using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services
{
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionExpirationService> _logger;

        public SubscriptionExpirationService(IServiceProvider serviceProvider, ILogger<SubscriptionExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription expiration service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var expiredSubscriptions = await unitOfWork.GetRepository<Subscription, Guid>()
                        .GetAllAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking expired subscriptions");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
