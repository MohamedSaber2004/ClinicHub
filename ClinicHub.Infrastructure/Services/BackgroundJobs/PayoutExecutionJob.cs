using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.Commands.DisbursePayoutItem;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Turns an approved batch into Paymob transfers. Money leaves the platform here, so the
/// budget is re-read at execution time (an approval can sit for days) and every item goes
/// through <see cref="DisbursePayoutItemCommandHandler"/> — the only code allowed to send a
/// transfer, which proves earlier attempts by inquiry before repeating one.
/// </summary>
public class PayoutExecutionJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PayoutExecutionJob> _logger;

    public PayoutExecutionJob(IServiceProvider serviceProvider, ILogger<PayoutExecutionJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [DisableConcurrentExecution(60 * 60)]
    public async Task ExecuteAsync(Guid batchId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var payoutService = scope.ServiceProvider.GetRequiredService<IPayoutService>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<PayoutSettings>>().Value;
        var batchRepository = unitOfWork.GetRepository<PayoutBatch, Guid>();

        var batch = await batchRepository.GetAllAsync(b => b.Id == batchId)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);

        if (batch is null)
        {
            _logger.LogWarning("Payout execution skipped: batch {BatchId} no longer exists.", batchId);
            return;
        }

        if (batch.Status == PayoutBatchStatus.Approved)
        {
            var budget = await payoutService.GetBudgetAsync(cancellationToken);
            if (!budget.Success)
            {
                batch.NoteFailure($"Payout budget could not be read: {budget.StatusDescription}");
                batchRepository.Update(batch);
                await unitOfWork.SaveChangesAsync();
                _logger.LogError("Payout batch {BatchCode} not executed: the Paymob budget did not answer.", batch.Code);
                return;
            }

            if (budget.Balance < batch.TotalAmount)
            {
                batch.NoteFailure($"Insufficient payout budget: {budget.Balance:0.00} available, {batch.TotalAmount:0.00} required.");
                batchRepository.Update(batch);
                await unitOfWork.SaveChangesAsync();
                _logger.LogWarning(
                    "Payout batch {BatchCode} not executed: budget {Balance:0.00} is below {Total:0.00}.",
                    batch.Code, budget.Balance, batch.TotalAmount);
                return;
            }

            if (!batch.StartExecution(budget.Balance))
                return;

            batchRepository.Update(batch);
            await unitOfWork.SaveChangesAsync();
        }
        else if (batch.Status != PayoutBatchStatus.Processing)
        {
            return;
        }

        var pending = batch.Items
            .Where(i => !i.IsResolved)
            .OrderBy(i => i.Amount)
            .ToList();

        _logger.LogInformation("Payout batch {BatchCode} executing {Count} item(s).", batch.Code, pending.Count);

        foreach (var item in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var outcome = await mediator.Send(new DisbursePayoutItemCommand { ItemId = item.Id }, cancellationToken);
            _logger.LogInformation("Payout item {ItemId} of batch {BatchCode} → {Outcome}.", item.Id, batch.Code, outcome);
        }

        // Wallet/bank results land asynchronously: give Paymob a couple of minutes, then ask.
        BackgroundJob.Schedule<PayoutStatusReconciliationJob>(
            job => job.ReconcileBatchAsync(batchId, CancellationToken.None),
            TimeSpan.FromMinutes(2));
    }
}
