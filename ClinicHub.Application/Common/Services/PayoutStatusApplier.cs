using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Common.Services;

public class PayoutStatusApplier : IPayoutStatusApplier
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClinicLedgerService _ledger;
    private readonly IPayoutService _payoutService;
    private readonly IFcmService _fcmService;
    private readonly ILogger<PayoutStatusApplier> _logger;

    public PayoutStatusApplier(
        IUnitOfWork unitOfWork,
        IClinicLedgerService ledger,
        IPayoutService payoutService,
        IFcmService fcmService,
        ILogger<PayoutStatusApplier> logger)
    {
        _unitOfWork = unitOfWork;
        _ledger = ledger;
        _payoutService = payoutService;
        _fcmService = fcmService;
        _logger = logger;
    }

    public async Task<bool> ApplyByTransactionIdAsync(
        string paymobTransactionId,
        CancellationToken cancellationToken = default)
    {
        var statuses = await _payoutService.InquireAsync(new[] { paymobTransactionId }, cancellationToken);

        var status = statuses.FirstOrDefault(s => s.TransactionId == paymobTransactionId);
        return status is null ? false : await ApplyAsync(status, cancellationToken);
    }

    public async Task<bool> ApplyAsync(
        PayoutTransactionStatus status,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<PayoutItem, Guid>();

        var item = await repository
            .GetAllAsync(i => i.PaymobTransactionId == status.TransactionId)
            .Include(i => i.Clinic)
            .FirstOrDefaultAsync(cancellationToken);

        // A disbursement whose answer we lost has no stored transaction id; the client
        // reference (the payout item id) is what ties Paymob's record back to ours.
        if (item is null && Guid.TryParse(status.ClientReferenceId, out var referenceId))
        {
            item = await repository.GetAllAsync(i => i.Id == referenceId)
                .Include(i => i.Clinic)
                .FirstOrDefaultAsync(cancellationToken);
            if (item is not null && item.PaymobTransactionId is null && !string.IsNullOrWhiteSpace(status.TransactionId))
            {
                item.MarkSent(status.TransactionId, status.DisbursementStatus, status.StatusCode);
                repository.Update(item);
            }
        }

        // A transaction id we never stored belongs to somebody else's account — ignore it.
        if (item is null)
            return false;

        if (status.IsSuccessful)
        {
            if (item.Status == PayoutItemStatus.Success)
                return false;

            item.MarkSuccess(status.StatusCode, status.DisbursementStatus);
            repository.Update(item);
            await _ledger.SettlePayoutItemAsync(item, cancellationToken);
            await NotifyClinicPaidAsync(item);
            return true;
        }

        if (status.IsFailed)
        {
            if (item.Status == PayoutItemStatus.Failed)
                return false;

            item.MarkFailed(status.StatusCode, status.DisbursementStatus, status.StatusDescription);
            repository.Update(item);
            await _ledger.ReleasePayoutItemAsync(item.Id, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task<bool> CloseBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<PayoutBatch, Guid>();

        var batch = await repository.GetAllAsync(b => b.Id == batchId)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);

        // The budget endpoint is throttled to 5 requests/minute, so it is read only for a
        // run that is genuinely over, not for every sweep that touches one of its items.
        if (batch is null
            || batch.Status != PayoutBatchStatus.Processing
            || batch.Items.Any(i => !i.IsResolved))
            return false;

        var budget = await _payoutService.GetBudgetAsync(cancellationToken);
        if (!batch.Complete(budget.Success ? budget.Balance : (decimal?)null))
            return false;

        repository.Update(batch);
        return true;
    }

    /// <summary>
    /// A lost notification must not roll back a transfer that Paymob already completed,
    /// so failures here are logged and swallowed like everywhere else in the settlement path.
    /// </summary>
    private async Task NotifyClinicPaidAsync(PayoutItem item)
    {
        if (item.Clinic?.ClinicAdminId is not Guid adminId)
            return;

        try
        {
            await _fcmService.SendToUserAsync(adminId, NotificationType.PayoutReceived, new()
            {
                ["clinicName"] = item.Clinic.Name,
                ["amount"] = $"{item.Amount:N2} {item.Currency}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payout-received notification for payout item {ItemId}.", item.Id);
        }
    }
}
