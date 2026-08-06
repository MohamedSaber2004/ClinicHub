using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class AdExpirationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AdExpirationJob> _logger;

    public AdExpirationJob(IServiceProvider serviceProvider, ILogger<AdExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task MarkExpiredAsync(Guid adId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var ad = await unitOfWork.GetRepository<Advertisement, Guid>().FindByKeyAsync(adId, cancellationToken);
        if (ad == null || ad.Status != AdvertisementStatus.Active)
            return;

        if (ad.EndDate > DateTime.Now)
            return;

        ad.Status = AdvertisementStatus.Expired;
        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Ad {AdId} marked as expired.", adId);
    }

    public async Task SweepExpiredAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredAds = await unitOfWork.GetRepository<Advertisement, Guid>()
            .GetAllAsync(a => a.Status == AdvertisementStatus.Active && a.EndDate <= DateTime.Now)
            .ToListAsync(cancellationToken);

        foreach (var ad in expiredAds)
        {
            ad.Status = AdvertisementStatus.Expired;
        }

        if (expiredAds.Count > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} ads", expiredAds.Count);
        }
    }
}
