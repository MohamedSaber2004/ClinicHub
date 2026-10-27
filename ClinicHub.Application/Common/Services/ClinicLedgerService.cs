using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Common.Services;

public class ClinicLedgerService : IClinicLedgerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutSettings _payoutSettings;

    public ClinicLedgerService(IUnitOfWork unitOfWork, IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task AccrueAppointmentEarningAsync(
        Payment payment,
        decimal platformFeePercent,
        CancellationToken cancellationToken = default)
    {
        if (payment.Type != PaymentType.Appointment || !payment.AppointmentId.HasValue)
            return;

        var split = PaymentRevenueResolver.Resolve(payment, platformFeePercent);
        payment.SnapshotRevenueSplit(split.PlatformFee, split.ClinicNet);

        var alreadyAccrued = await _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>()
            .GetAllAsync(e => e.PaymentId == payment.Id && e.Type == LedgerEntryType.Earning)
            .AnyAsync(cancellationToken);

        if (alreadyAccrued)
            return;

        var availableAt = (payment.PaidAt ?? DateTime.Now).AddDays(_payoutSettings.RefundHoldDays);
        var entry = ClinicLedgerEntry.CreateEarning(
            payment.ClinicId,
            payment.Id,
            split.ClinicNet,
            payment.Currency,
            availableAt);

        await _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>().AddAsync(entry);
    }

    public async Task<decimal> ReserveForPayoutItemAsync(
        Guid clinicId,
        Guid payoutItemId,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>();
        var now = DateTime.Now;

        // Refund deductions are Available and negative, so the sum is the net the clinic is
        // actually owed rather than its gross earnings.
        var entries = await repository
            .GetAllAsync(e => e.ClinicId == clinicId
                && e.Status == LedgerEntryStatus.Available
                && e.AvailableAt <= now)
            .ToListAsync(cancellationToken);

        var total = entries.Sum(e => e.Amount);
        if (total <= 0m)
            return 0m;

        foreach (var entry in entries)
        {
            entry.Reserve(payoutItemId);
            repository.Update(entry);
        }

        return total;
    }

    public async Task SettlePayoutItemAsync(PayoutItem item, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>();

        var reserved = await repository
            .GetAllAsync(e => e.PayoutItemId == item.Id && e.Status == LedgerEntryStatus.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var entry in reserved)
        {
            entry.Settle();
            repository.Update(entry);
        }

        await repository.AddAsync(
            ClinicLedgerEntry.CreatePayout(item.ClinicId, item.Id, item.Amount, item.Currency));
    }

    public async Task ReleasePayoutItemAsync(Guid payoutItemId, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>();

        var reserved = await repository
            .GetAllAsync(e => e.PayoutItemId == payoutItemId && e.Status == LedgerEntryStatus.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var entry in reserved)
        {
            entry.Release();
            repository.Update(entry);
        }
    }
}
