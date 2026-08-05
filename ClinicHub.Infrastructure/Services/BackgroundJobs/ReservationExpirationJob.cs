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
        await unitOfWork.SaveChangesAsync();
        await NotifyPatientAsync(scope.ServiceProvider, appointment);
        _logger.LogInformation("Reservation {AppointmentId} expired (payment not completed within TTL).", appointmentId);
    }

    public async Task SweepExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredReservations = await unitOfWork.AppointmentRepository
            .GetAllAsync(a => a.Status == AppointmentStatus.Reserved && a.ExpiresAt <= DateTime.UtcNow)
            .Include(a => a.Clinic)
            .ToListAsync(cancellationToken);

        foreach (var appointment in expiredReservations)
        {
            appointment.ExpireReservation();
        }

        if (expiredReservations.Count > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} reservations", expiredReservations.Count);
        }

        foreach (var appointment in expiredReservations)
        {
            await NotifyPatientAsync(scope.ServiceProvider, appointment);
        }
    }

    private static async Task NotifyPatientAsync(IServiceProvider serviceProvider, Appointment appointment)
    {
        var fcmService = serviceProvider.GetRequiredService<IFcmService>();
        await fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
        {
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["reason"] = "لم يتم تأكيد الحجز خلال المهلة المحددة"
        });
    }
}
