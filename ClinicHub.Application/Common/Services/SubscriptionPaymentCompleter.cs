using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
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

    public SubscriptionPaymentCompleter(
        IUnitOfWork unitOfWork,
        IBackgroundJobScheduler jobScheduler,
        ILogger<SubscriptionPaymentCompleter> logger)
    {
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
        _logger = logger;
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

        return subscription.Id;
    }
}
