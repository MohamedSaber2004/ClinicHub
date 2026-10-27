using ClinicHub.Application.Features.AdminPayouts.Commands.ReconcilePayoutItems;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Paymob settles transfers asynchronously and its callback is a hint without a signature,
/// so the truth is always pulled from the inquiry endpoint. This job is both the short-delay
/// follow-up of a batch run and the hourly safety net for anything a callback missed.
/// </summary>
public class PayoutStatusReconciliationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PayoutStatusReconciliationJob> _logger;

    public PayoutStatusReconciliationJob(
        IServiceProvider serviceProvider,
        ILogger<PayoutStatusReconciliationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ReconcileBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        await RunAsync(batchId, cancellationToken);
    }

    public async Task SweepAsync(CancellationToken cancellationToken)
    {
        await RunAsync(null, cancellationToken);
    }

    private async Task RunAsync(Guid? batchId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            var resolved = await mediator.Send(
                new ReconcilePayoutItemsCommand { BatchId = batchId }, cancellationToken);

            if (resolved > 0)
                _logger.LogInformation("Payout reconciliation confirmed {Resolved} disbursement(s).", resolved);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payout reconciliation sweep failed for batch {BatchId}.", batchId);
        }
    }
}
