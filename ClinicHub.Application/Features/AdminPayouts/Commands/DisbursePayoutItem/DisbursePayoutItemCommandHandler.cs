using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.DisbursePayoutItem;

public class DisbursePayoutItemCommandHandler
    : IRequestHandler<DisbursePayoutItemCommand, PayoutDisbursementOutcome>
{
    private enum ProvenResult
    {
        Absent,
        Successful,
        Failed,
        Pending
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayoutService _payoutService;
    private readonly IPayoutStatusApplier _statusApplier;
    private readonly IClinicLedgerService _ledger;
    private readonly IPayoutAccountProtector _protector;
    private readonly PayoutSettings _payoutSettings;
    private readonly ILogger<DisbursePayoutItemCommandHandler> _logger;

    public DisbursePayoutItemCommandHandler(
        IUnitOfWork unitOfWork,
        IPayoutService payoutService,
        IPayoutStatusApplier statusApplier,
        IClinicLedgerService ledger,
        IPayoutAccountProtector protector,
        IOptions<PayoutSettings> payoutSettings,
        ILogger<DisbursePayoutItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _payoutService = payoutService;
        _statusApplier = statusApplier;
        _ledger = ledger;
        _protector = protector;
        _payoutSettings = payoutSettings.Value;
        _logger = logger;
    }

    public async Task<PayoutDisbursementOutcome> Handle(
        DisbursePayoutItemCommand request,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutItem, Guid>();

        var item = await repository.GetAllAsync(i => i.Id == request.ItemId)
            .Include(i => i.Batch)
            .Include(i => i.PayoutAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.ItemNotFound.Value);

        if (item.IsResolved || item.Batch.Status != PayoutBatchStatus.Processing)
            return PayoutDisbursementOutcome.Skipped;

        // Any earlier attempt whose answer we lost must be proven absent before a second
        // transfer is even possible.
        if (item.Attempts > 0)
        {
            var proven = await ProveAsync(item, cancellationToken);
            switch (proven)
            {
                case ProvenResult.Pending:
                    return PayoutDisbursementOutcome.AwaitingConfirmation;
                case ProvenResult.Successful:
                case ProvenResult.Failed:
                    return PayoutDisbursementOutcome.Skipped;
            }
        }

        if (!TryReadDestination(item.PayoutAccount, out var accountNumber, out var nationalId, out var destinationError))
            return await RejectAsync(item, destinationError, cancellationToken);

        var result = await _payoutService.DisburseBankAsync(new PayoutBankDisbursementRequest
        {
            FullName = item.PayoutAccount.HolderName,
            Amount = item.Amount,
            BankCardNumber = accountNumber,
            BankCode = item.PayoutAccount.BankCode,
            BankTransactionType = item.PayoutAccount.BankTransactionType,
            NationalId = nationalId,
            ClientReferenceId = item.Id
        }, cancellationToken);

        switch (result.Outcome)
        {
            case PayoutOutcome.Accepted:
                item.MarkSent(result.TransactionId!, result.DisbursementStatus, result.StatusCode);
                repository.Update(item);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                    "Payout item {ItemId} (clinic {ClinicId}) disbursed: {Amount:0.00} {Currency}, Paymob transaction {TransactionId}.",
                    item.Id, item.ClinicId, item.Amount, item.Currency, result.TransactionId);
                return PayoutDisbursementOutcome.Sent;

            case PayoutOutcome.Rejected:
                return await RejectAsync(
                    item,
                    result.StatusDescription ?? $"Paymob status code {result.StatusCode}",
                    cancellationToken);

            default:
                item.NoteAttempt(result.StatusCode, result.DisbursementStatus,
                    "Paymob did not answer; this item is inquired before it can be retried.");
                repository.Update(item);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogWarning(
                    "Payout item {ItemId} disbursement is unconfirmed after {Attempts} attempt(s).",
                    item.Id, item.Attempts);
                return PayoutDisbursementOutcome.AwaitingConfirmation;
        }
    }

    private async Task<PayoutDisbursementOutcome> RejectAsync(
        PayoutItem item,
        string? reason,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutItem, Guid>();

        item.MarkFailed(null, null, reason ?? "Paymob refused the transfer.");
        repository.Update(item);

        // Money stays reserved while the item can still be retried. It goes back to the
        // clinic's balance only once retrying is impossible — safe because a refused
        // transfer never got a Paymob transaction id, so nothing can confirm later.
        if (!item.CanRetry(_payoutSettings.MaxRetriesPerItem))
            await _ledger.ReleasePayoutItemAsync(item.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Payout item {ItemId} refused: {Reason}", item.Id, reason);
        return PayoutDisbursementOutcome.Rejected;
    }

    private async Task<ProvenResult> ProveAsync(PayoutItem item, CancellationToken cancellationToken)
    {
        var statuses = item.PaymobTransactionId is not null
            ? await _payoutService.InquireAsync(new[] { item.PaymobTransactionId }, cancellationToken)
            : await _payoutService.InquireByReferenceAsync(new[] { item.Id.ToString() }, cancellationToken);

        var status = statuses.FirstOrDefault();
        if (status is null)
            return ProvenResult.Absent;

        // The reference inquiry can reveal a transaction we lost the answer for; adopt its id
        // so the status applier can find this item.
        if (item.PaymobTransactionId is null && !string.IsNullOrWhiteSpace(status.TransactionId))
        {
            item.MarkSent(status.TransactionId, status.DisbursementStatus, status.StatusCode);
            _unitOfWork.GetRepository<PayoutItem, Guid>().Update(item);
            await _unitOfWork.SaveChangesAsync();
        }

        if (status.IsSuccessful || status.IsFailed)
        {
            await _statusApplier.ApplyAsync(status, cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            return status.IsSuccessful ? ProvenResult.Successful : ProvenResult.Failed;
        }

        item.NoteAttempt(status.StatusCode, status.DisbursementStatus);
        _unitOfWork.GetRepository<PayoutItem, Guid>().Update(item);
        await _unitOfWork.SaveChangesAsync();
        return ProvenResult.Pending;
    }

    private bool TryReadDestination(
        ClinicPayoutAccount account,
        out string accountNumber,
        out string nationalId,
        out string? error)
    {
        accountNumber = string.Empty;
        nationalId = string.Empty;
        error = null;

        if (account.Status != PayoutAccountStatus.Verified)
        {
            error = "Payout account is not verified.";
            return false;
        }

        try
        {
            accountNumber = _protector.Unprotect(account.AccountNumberEncrypted);
            nationalId = _protector.Unprotect(account.NationalIdEncrypted);
        }
        catch (Exception ex)
        {
            // Losing the DataProtection keys must never turn into a transfer to a wrong account.
            error = $"Stored bank details could not be decrypted ({ex.GetType().Name}).";
            return false;
        }

        if (string.IsNullOrWhiteSpace(accountNumber) || string.IsNullOrWhiteSpace(nationalId))
        {
            error = "Payout account is missing its bank account number or national id.";
            return false;
        }

        return true;
    }
}
