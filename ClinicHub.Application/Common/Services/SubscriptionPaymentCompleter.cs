using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Common.Services;

/// <summary>
/// Shared subscription activation flow used by both the Paymob webhook handler
/// and the post-redirect payment verification query, so a paid subscription
/// payment always ends up with exactly one active subscription.
/// </summary>
public class SubscriptionPaymentCompleter : ISubscriptionPaymentCompleter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobScheduler _jobScheduler;
    private readonly ILogger<SubscriptionPaymentCompleter> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFcmService _fcmService;

    public SubscriptionPaymentCompleter(
        IUnitOfWork unitOfWork,
        IBackgroundJobScheduler jobScheduler,
        ILogger<SubscriptionPaymentCompleter> logger,
        UserManager<ApplicationUser> userManager,
        IFcmService fcmService)
    {
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
        _logger = logger;
        _userManager = userManager;
        _fcmService = fcmService;
    }

    public async Task<Guid> ActivateFromPaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (payment.SubscriptionId.HasValue)
        {
            _logger.LogWarning("Payment {PaymentId} already has subscription {SubscriptionId}. Skipping duplicate.",
                payment.Id, payment.SubscriptionId);
            return payment.SubscriptionId.Value;
        }

        var existingActiveSubs = await _unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.ClinicId == payment.ClinicId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var activeSub in existingActiveSubs)
        {
            activeSub.Status = SubscriptionStatus.Revoked;
            activeSub.Notes = "Revoked due to new subscription payment confirmation.";
        }

        var period = payment.SubscriptionPeriod ?? SubscriptionPlan.Monthly;
        var now = DateTime.Now;
        var endDate = period == SubscriptionPlan.Yearly ? now.AddYears(1) : now.AddMonths(1);

        var subscription = new Subscription
        {
            ClinicId = payment.ClinicId,
            PlanId = payment.PlanId,
            Period = period,
            StartDate = now,
            EndDate = endDate,
            Amount = payment.Amount,
            Status = SubscriptionStatus.Active,
            PaidAt = now,
            PaymentId = payment.Id
        };

        await _unitOfWork.GetRepository<Subscription, Guid>().AddAsync(subscription);
        payment.LinkToSubscription(subscription.Id);
        await _jobScheduler.ScheduleSubscriptionExpirationAsync(subscription.Id, subscription.EndDate);

        // Notifications must never break the financial transaction; failures are logged only.
        try
        {
            await NotifySuperAdminsAndClinicOwnerAsync(payment, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subscription activation notifications failed for payment {PaymentId}; activation is unaffected.", payment.Id);
        }

        return subscription.Id;
    }

    private async Task NotifySuperAdminsAndClinicOwnerAsync(Payment payment, CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(payment.ClinicId);
        if (clinic == null) return;

        string planName = "";
        if (payment.PlanId.HasValue)
        {
            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(payment.PlanId.Value);
            planName = plan?.Name ?? "";
        }

        var parameters = new Dictionary<string, object>
        {
            ["clinicName"] = clinic.Name ?? "",
            ["planName"] = planName,
            ["amount"] = $"{payment.Amount:N2} EGP"
        };

        var superAdmins = await _userManager.GetUsersInRoleAsync(UserType.SuperAdmin.ToString());
        foreach (var admin in superAdmins.Where(a => !a.IsDeleted))
        {
            await _fcmService.SendToUserAsync(admin.Id, NotificationType.SubscriptionActivated, parameters);
        }

        if (clinic.ClinicAdminId.HasValue)
        {
            await _fcmService.SendToUserAsync(clinic.ClinicAdminId.Value, NotificationType.SubscriptionActivated, parameters);
        }
    }
}
