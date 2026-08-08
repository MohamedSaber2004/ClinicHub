using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class DoctorAvailabilityValidationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DoctorAvailabilityValidationJob> _logger;

    public DoctorAvailabilityValidationJob(IServiceProvider serviceProvider, ILogger<DoctorAvailabilityValidationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SweepAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

        var now = DateTime.Now;
        var fromDate = now.Date;
        var toDate = fromDate.Add(AvailabilityValidation.ValidationHorizon);

        var appointments = await unitOfWork.AppointmentRepository
            .GetAllAsync(a => a.AppointmentDate >= fromDate
                && a.AppointmentDate <= toDate
                && AvailabilityValidation.ActiveStatuses.Contains(a.Status))
            .Include(a => a.Doctor)
            .Include(a => a.Clinic)
            .ToListAsync(cancellationToken);

        var availabilities = await unitOfWork.DoctorAvailabilityRepository
            .GetAllAsync(a => a.IsDeleted == false)
            .ToListAsync(cancellationToken);

        var availabilityByDoctor = availabilities
            .GroupBy(a => (a.DoctorId, a.ClinicId))
            .ToDictionary(g => g.Key, g => g.ToList());

        var sentKeys = await AvailabilityValidation.GetRecentlyNotifiedKeysAsync(
            unitOfWork, [NotificationType.AppointmentOutsideAvailability], now, cancellationToken);

        var adminByClinic = await AvailabilityValidation.ResolveClinicAdminIdsAsync(
            unitOfWork, appointments.Select(a => a.Clinic), cancellationToken);

        var flagged = 0;

        foreach (var appointment in appointments)
        {
            if (!IsWithinAvailability(appointment, availabilityByDoctor))
            {
                flagged++;
                await NotifyAsync(unitOfWork, fcmService, appointment, adminByClinic, sentKeys, now, cancellationToken);
            }
        }

        _logger.LogInformation(
            "Doctor availability validation: checked {AppointmentCount} upcoming appointments, flagged {FlaggedCount} outside doctor availability.",
            appointments.Count, flagged);

        // Single commit: all notification rows added by the sends above in one transaction.
        await unitOfWork.SaveChangesAsync();
    }

    private static bool IsWithinAvailability(
        Appointment appointment,
        Dictionary<(Guid DoctorId, Guid ClinicId), List<DoctorAvailability>> availabilityByDoctor)
    {
        if (!availabilityByDoctor.TryGetValue((appointment.DoctorId, appointment.ClinicId), out var availabilities))
            return false;

        return availabilities.Any(a =>
            a.DayOfWeek == appointment.AppointmentDate.DayOfWeek
            && a.StartTime <= appointment.StartTime
            && a.EndTime >= appointment.EndTime);
    }

    private async Task NotifyAsync(
        IUnitOfWork unitOfWork,
        IFcmService fcmService,
        Appointment appointment,
        Dictionary<Guid, Guid> adminByClinic,
        HashSet<(Guid UserId, NotificationType Type)> sentKeys,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var recipients = new List<(Guid UserId, NotificationType Type)>
        {
            (appointment.Doctor.UserId, NotificationType.AppointmentOutsideAvailability)
        };

        if (appointment.Clinic != null && adminByClinic.TryGetValue(appointment.Clinic.Id, out var adminId))
            recipients.Add((adminId, NotificationType.AppointmentOutsideAvailability));

        foreach (var (userId, type) in recipients)
        {
            if (!sentKeys.Add((userId, type)))
                continue;

            try
            {
                await fcmService.SendToUserAsync(userId, type, new()
                {
                    ["clinicName"] = appointment.Clinic?.Name ?? "",
                    ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                    ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
                    ["appointmentId"] = appointment.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send outside-availability notification for appointment {AppointmentId}.", appointment.Id);
            }
        }
    }
}
