using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class AbandonedPaymentJob
{
    private static readonly TimeSpan AbandonmentThreshold = TimeSpan.FromHours(24);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AbandonedPaymentJob> _logger;

    public AbandonedPaymentJob(IServiceProvider serviceProvider, ILogger<AbandonedPaymentJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SweepAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cutoff = DateTime.Now.Add(-AbandonmentThreshold);

        var abandonedPayments = await unitOfWork.PaymentRepository
            .GetAllAsync(p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
                && p.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);

        foreach (var payment in abandonedPayments)
        {
            // Record the abandoned checkout as failed, but never cancel the appointment:
            // a Reserved appointment stays in the staff queue until staff accepts,
            // rejects, or cancels it.
            payment.MarkAsFailed("Abandoned - no payment confirmation within 24 hours.");

            _logger.LogInformation("Payment {PaymentId} marked as failed (abandoned checkout).", payment.Id);
        }

        // Single commit: all failed-payment rows in one transaction.
        await unitOfWork.SaveChangesAsync();
    }
}
