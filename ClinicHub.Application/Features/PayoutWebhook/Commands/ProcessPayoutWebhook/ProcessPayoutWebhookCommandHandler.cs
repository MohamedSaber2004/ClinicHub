using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.PayoutWebhook.Commands.ProcessPayoutWebhook;

public class ProcessPayoutWebhookCommandHandler
    : IRequestHandler<ProcessPayoutWebhookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayoutStatusApplier _statusApplier;
    private readonly ILogger<ProcessPayoutWebhookCommandHandler> _logger;

    public ProcessPayoutWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPayoutStatusApplier statusApplier,
        ILogger<ProcessPayoutWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _statusApplier = statusApplier;
        _logger = logger;
    }

    public async Task<bool> Handle(
        ProcessPayoutWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutWebhookEvent, Guid>();

        var known = await repository
            .GetAllAsync(e => e.PaymobTransactionId == request.TransactionId)
            .AnyAsync(cancellationToken);

        if (known)
            return true;

        var payoutEvent = PayoutWebhookEvent.Create(
            request.TransactionId,
            request.Issuer,
            request.Amount,
            request.StatusCode,
            request.DisbursementStatus,
            request.RawPayload);

        await repository.AddAsync(payoutEvent);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _statusApplier.ApplyByTransactionIdAsync(request.TransactionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            payoutEvent.MarkProcessed();
            repository.Update(payoutEvent);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The callback stays on record; the hourly reconciliation sweep retries it.
            _logger.LogError(ex, "Payout callback {TransactionId} could not be confirmed against Paymob.",
                request.TransactionId);
            payoutEvent.MarkFailed(ex.Message);
            repository.Update(payoutEvent);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
