using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class ExpiryReminderJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiryReminderJob> _logger;

    public ExpiryReminderJob(IServiceProvider serviceProvider, ILogger<ExpiryReminderJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendExpiryRemindersAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

        var now = DateTime.Now;
        var subscriptionReminders = 0;
        var adReminders = 0;

        var subscriptions = await unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > now)
            .Include(s => s.Clinic)
            .ToListAsync(cancellationToken);

        foreach (var subscription in subscriptions)
        {
            var remaining = subscription.EndDate - now;

            if (remaining > TimeSpan.FromDays(2) && remaining <= TimeSpan.FromDays(3))
            {
                await NotifyClinicOwnerAsync(unitOfWork, fcmService, subscription.Clinic, subscription.EndDate,
                    NotificationType.SubscriptionExpiring, "Ù£ Ø£ÙŠØ§Ù…", cancellationToken);
                subscriptionReminders++;
            }
            else if (remaining > TimeSpan.Zero && remaining <= TimeSpan.FromDays(1))
            {
                await NotifyClinicOwnerAsync(unitOfWork, fcmService, subscription.Clinic, subscription.EndDate,
                    NotificationType.SubscriptionExpiring, "ÙŠÙˆÙ… ÙˆØ§Ø­Ø¯", cancellationToken);
                subscriptionReminders++;
            }
        }

        var ads = await unitOfWork.GetRepository<Advertisement, Guid>()
            .GetAllAsync(a => a.Status == AdvertisementStatus.Active && a.EndDate > now)
            .Include(a => a.Clinic)
            .ToListAsync(cancellationToken);

        foreach (var ad in ads)
        {
            var remaining = ad.EndDate - now;

            if (remaining > TimeSpan.FromDays(2) && remaining <= TimeSpan.FromDays(3))
            {
                await NotifyClinicOwnerAsync(unitOfWork, fcmService, ad.Clinic, ad.EndDate,
                    NotificationType.AdExpiring, "Ù£ Ø£ÙŠØ§Ù…", cancellationToken);
                adReminders++;
            }
            else if (remaining > TimeSpan.Zero && remaining <= TimeSpan.FromDays(1))
            {
                await NotifyClinicOwnerAsync(unitOfWork, fcmService, ad.Clinic, ad.EndDate,
                    NotificationType.AdExpiring, "ÙŠÙˆÙ… ÙˆØ§Ø­Ø¯", cancellationToken);
                adReminders++;
            }
        }

        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Expiry reminders sent: {SubscriptionCount} subscriptions, {AdCount} ads in reminder windows.",
            subscriptionReminders, adReminders);
    }

    private static async Task NotifyClinicOwnerAsync(
        IUnitOfWork unitOfWork,
        IFcmService fcmService,
        Clinic? clinic,
        DateTime endDate,
        NotificationType type,
        string period,
        CancellationToken cancellationToken)
    {
        if (clinic == null)
            return;

        var user = await unitOfWork.GetRepository<ApplicationUser, Guid>()
            .GetAllAsync(u => u.ClinicId == clinic.Id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        var userId = user?.Id ?? clinic.ClinicAdminId;
        if (!userId.HasValue)
            return;

        await fcmService.SendToUserAsync(userId.Value, type, new()
        {
            ["clinicName"] = clinic.Name ?? "",
            ["date"] = endDate.ToString("yyyy-MM-dd"),
            ["period"] = period
        });
    }
}
