using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
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
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

        var cutoff = DateTime.Now.Add(-AbandonmentThreshold);

        var abandonedPayments = await unitOfWork.PaymentRepository
            .GetAllAsync(p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
                && p.CreatedAt < cutoff)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Clinic)
            .ToListAsync(cancellationToken);

        var cancelledAppointments = new List<Appointment>();

        foreach (var payment in abandonedPayments)
        {
            payment.MarkAsFailed("Abandoned - no payment confirmation within 24 hours.");

            if (payment.Type == PaymentType.Appointment
                && payment.Appointment != null
                && payment.Appointment.Status == AppointmentStatus.Reserved)
            {
                payment.Appointment.ExpireReservation();
                cancelledAppointments.Add(payment.Appointment);
            }

            _logger.LogInformation("Payment {PaymentId} marked as failed (abandoned checkout).", payment.Id);
        }

        if (abandonedPayments.Count > 0)
        {
            await unitOfWork.SaveChangesAsync();
        }

        foreach (var appointment in cancelledAppointments)
        {
            await fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
            {
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["reason"] = "Ù„Ù… ÙŠØªÙ… ØªØ£ÙƒÙŠØ¯ Ø§Ù„Ø­Ø¬Ø² Ø®Ù„Ø§Ù„ Ø§Ù„Ù…Ù‡Ù„Ø© Ø§Ù„Ù…Ø­Ø¯Ø¯Ø©"
            });
        }
    }
}
