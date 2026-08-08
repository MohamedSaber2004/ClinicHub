using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class RefundRetryJob
{
    private const int MaxAttempts = 4;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefundRetryJob> _logger;

    public RefundRetryJob(IServiceProvider serviceProvider, ILogger<RefundRetryJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RetryRefundAsync(Guid paymentId, int attempt, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var paymobService = scope.ServiceProvider.GetRequiredService<IPaymobService>();
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

        var payment = await unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Clinic)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null || payment.Status != PaymentStatus.Paid)
            return;

        if (string.IsNullOrWhiteSpace(payment.PaymobTransactionId))
        {
            _logger.LogWarning("Refund retry skipped for payment {PaymentId}: missing Paymob transaction id.", paymentId);
            return;
        }

        var refundResult = await PaymentRefundGate.RunAsync(paymentId, async () =>
        {
            // Re-check against the DB: the refund may have succeeded elsewhere
            // (user re-cancel, admin refund) while this retry was in flight.
            var alreadyRefunded = await unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
                .AsNoTracking()
                .AnyAsync(p => p.Status == PaymentStatus.Refunded, cancellationToken);
            if (alreadyRefunded)
                return new RefundResultDto { Success = true, Message = "Already refunded" };

            return await paymobService.RefundTransactionAsync(payment.PaymobTransactionId, payment.Amount, cancellationToken);
        });
        if (refundResult.Success)
        {
            payment.MarkAsRefunded("Refunded by background retry (user cancellation).");

            if (payment.AppointmentId.HasValue && payment.Appointment != null)
            {
                await fcmService.SendToUserAsync(payment.Appointment.BookedByUserId, NotificationType.RefundProcessed, new()
                {
                    ["clinicName"] = payment.Appointment.Clinic?.Name ?? "",
                    ["amount"] = $"{payment.Amount:N2} {payment.Currency}",
                    ["appointmentId"] = payment.Appointment.Id.ToString()
                });
            }

            // Single commit: the refund state and the notification row in one transaction.
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Refund retry succeeded for payment {PaymentId}.", paymentId);
            return;
        }

        if (attempt >= MaxAttempts)
        {
            _logger.LogError("Refund retries exhausted for payment {PaymentId} after {MaxAttempts} attempts.", paymentId, MaxAttempts);
            return;
        }

        var delay = TimeSpan.FromHours(Math.Pow(2, attempt));
        BackgroundJob.Schedule<RefundRetryJob>(job => job.RetryRefundAsync(paymentId, attempt + 1, CancellationToken.None), delay);
        _logger.LogWarning("Refund retry {Attempt} failed for payment {PaymentId}; rescheduled in {Delay} hours.", attempt, paymentId, delay.TotalHours);
    }
}
