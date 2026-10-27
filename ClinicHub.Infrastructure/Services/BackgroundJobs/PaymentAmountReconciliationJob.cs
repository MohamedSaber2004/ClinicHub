using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Keeps Payment.Amount equal to what Paymob actually collected. A re-issued checkout
/// (e.g. after the clinic changed its consultation fee) used to leave the previous amount
/// behind, and every revenue figure reads that column.
/// </summary>
public class PaymentAmountReconciliationJob
{
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromDays(7);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentAmountReconciliationJob> _logger;

    public PaymentAmountReconciliationJob(IServiceProvider serviceProvider, ILogger<PaymentAmountReconciliationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var paymob = scope.ServiceProvider.GetRequiredService<IPaymobService>();

        var cutoff = DateTime.Now.Subtract(LookbackWindow);

        var payments = await unitOfWork.PaymentRepository
            .GetAllAsync(p => p.Type == PaymentType.Appointment
                && p.Status == PaymentStatus.Paid
                && p.PaymobOrderId != null
                && p.PaymobOrderId != string.Empty
                && p.PaidAt >= cutoff)
            .ToListAsync(cancellationToken);

        var reconciled = 0;
        var percent = await GetPlatformFeePercentAsync(unitOfWork, cancellationToken);
        var ledgerRepository = unitOfWork.GetRepository<ClinicLedgerEntry, Guid>();

        foreach (var payment in payments)
        {
            var order = await paymob.GetOrderPaymentStatusAsync(payment.PaymobOrderId!, cancellationToken);
            if (!order.Found || order.PaidAmountCents <= 0)
                continue;

            var collected = order.PaidAmountCents / 100m;
            if (collected == payment.Amount)
                continue;

            _logger.LogWarning(
                "Payment {PaymentId} (order {OrderId}) stored {StoredAmount} but Paymob collected {CollectedAmount}; amount corrected.",
                payment.Id, payment.PaymobOrderId, payment.Amount, collected);

            payment.RefreshChargedAmount(collected);

            // The ledger is what payouts are computed from, so a corrected amount must
            // re-snapshot the split and revalue the (not yet reserved) earning row.
            var split = AppointmentRevenueSplitter.Split(collected, percent);
            payment.SnapshotRevenueSplit(split.PlatformFee, split.ClinicNet);

            var earning = await ledgerRepository
                .GetAllAsync(e => e.PaymentId == payment.Id && e.Type == LedgerEntryType.Earning)
                .FirstOrDefaultAsync(cancellationToken);

            earning?.TryRevalue(split.ClinicNet);
            reconciled++;
        }

        if (reconciled > 0)
            await unitOfWork.SaveChangesAsync();
    }

    private static async Task<decimal> GetPlatformFeePercentAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return setting?.AppointmentFeePercent ?? 0m;
    }
}
