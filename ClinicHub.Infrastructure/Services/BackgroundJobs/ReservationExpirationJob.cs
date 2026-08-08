using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class ReservationExpirationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationExpirationJob> _logger;

    public ReservationExpirationJob(IServiceProvider serviceProvider, ILogger<ReservationExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExpireReservationAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var appointment = await unitOfWork.AppointmentRepository
            .GetAllAsync(a => a.Id == appointmentId)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment == null || appointment.Status != AppointmentStatus.Reserved)
            return;

        if (!appointment.IsReservationExpired())
            return;

        appointment.ExpireReservation();
        await NotifyPatientAsync(scope.ServiceProvider, appointment);
        // Single commit: the expiration and the notification row in one transaction.
        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Reservation {AppointmentId} expired (payment not completed within TTL).", appointmentId);
    }

    public async Task SweepExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredReservations = await unitOfWork.AppointmentRepository
            .GetAllAsync(a => a.Status == AppointmentStatus.Reserved && a.ExpiresAt <= DateTime.Now)
            .Include(a => a.Clinic)
            .ToListAsync(cancellationToken);

        foreach (var appointment in expiredReservations)
        {
            appointment.ExpireReservation();
        }

        if (expiredReservations.Count > 0)
        {
            foreach (var appointment in expiredReservations)
            {
                await NotifyPatientAsync(scope.ServiceProvider, appointment);
            }

            // Single commit: all expirations and notification rows in one transaction.
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} reservations", expiredReservations.Count);
        }
    }

    private static async Task NotifyPatientAsync(IServiceProvider serviceProvider, Appointment appointment)
    {
        var fcmService = serviceProvider.GetRequiredService<IFcmService>();
        await fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
        {
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["reason"] = "Ù„Ù… ÙŠØªÙ… ØªØ£ÙƒÙŠØ¯ Ø§Ù„Ø­Ø¬Ø² Ø®Ù„Ø§Ù„ Ø§Ù„Ù…Ù‡Ù„Ø© Ø§Ù„Ù…Ø­Ø¯Ø¯Ø©"
        });
    }
}
