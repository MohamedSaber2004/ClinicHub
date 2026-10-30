using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ReconcilePayoutItems;

public class ReconcilePayoutItemsCommandHandler
    : IRequestHandler<ReconcilePayoutItemsCommand, int>
{
    /// <summary>Paymob paginates inquiry results at 50 transactions and throttles to 5 requests/minute.</summary>
    private const int InquiryChunkSize = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayoutService _payoutService;
    private readonly IPayoutStatusApplier _statusApplier;
    private readonly ILogger<ReconcilePayoutItemsCommandHandler> _logger;

    public ReconcilePayoutItemsCommandHandler(
        IUnitOfWork unitOfWork,
        IPayoutService payoutService,
        IPayoutStatusApplier statusApplier,
        ILogger<ReconcilePayoutItemsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _payoutService = payoutService;
        _statusApplier = statusApplier;
        _logger = logger;
    }

    public async Task<int> Handle(
        ReconcilePayoutItemsCommand request,
        CancellationToken cancellationToken)
    {
        var applied = await ApplySentItemsAsync(request, cancellationToken);
        applied += await ProveLostAnswersAsync(request, cancellationToken);

        var batches = await _unitOfWork.GetRepository<PayoutItem, Guid>()
            .GetAllAsync(i => i.Batch.Status == PayoutBatchStatus.Processing
                && (request.BatchId == null || i.BatchId == request.BatchId))
            .Select(i => i.BatchId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var batchId in batches)
            await TryCloseBatchAsync(batchId, cancellationToken);

        _logger.LogInformation("Payout reconciliation resolved {Applied} disbursement(s).", applied);

        return applied;
    }

    private async Task<int> ApplySentItemsAsync(
        ReconcilePayoutItemsCommand request,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutItem, Guid>();

        var sent = await repository
            .GetAllAsync(i => i.Status == PayoutItemStatus.Processing
                && i.PaymobTransactionId != null
                && (request.BatchId == null || i.BatchId == request.BatchId))
            .ToListAsync(cancellationToken);

        if (sent.Count == 0)
            return 0;

        var applied = 0;

        foreach (var chunk in sent.Chunk(InquiryChunkSize))
        {
            var transactionIds = chunk.Select(i => i.PaymobTransactionId!).Distinct().ToList();
            var statuses = await _payoutService.InquireAsync(transactionIds, cancellationToken);

            foreach (var status in statuses)
            {
                if (await _statusApplier.ApplyAsync(status, cancellationToken))
                    applied++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return applied;
    }

    /// <summary>
    /// Items whose disbursement answer was lost carry no transaction id, so they are found
    /// by the client reference we sent (the payout item id). Anything Paymob does not know
    /// about simply stays where it is.
    /// </summary>
    private async Task<int> ProveLostAnswersAsync(
        ReconcilePayoutItemsCommand request,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutItem, Guid>();

        var unanswered = await repository
            .GetAllAsync(i => i.Status == PayoutItemStatus.Pending
                && i.PaymobTransactionId == null
                && i.Attempts > 0
                && i.Batch.Status == PayoutBatchStatus.Processing
                && (request.BatchId == null || i.BatchId == request.BatchId))
            .ToListAsync(cancellationToken);

        if (unanswered.Count == 0)
            return 0;

        var applied = 0;

        foreach (var chunk in unanswered.Chunk(InquiryChunkSize))
        {
            var references = chunk.Select(i => i.Id.ToString()).ToList();
            var statuses = await _payoutService.InquireByReferenceAsync(references, cancellationToken);

            foreach (var status in statuses)
            {
                if (await _statusApplier.ApplyAsync(status, cancellationToken))
                    applied++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return applied;
    }

    private async Task TryCloseBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        await _statusApplier.CloseBatchAsync(batchId, cancellationToken);
        await _unitOfWork.SaveChangesAsync();
    }
}
