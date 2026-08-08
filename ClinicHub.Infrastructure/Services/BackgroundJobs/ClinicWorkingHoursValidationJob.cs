using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class ClinicWorkingHoursValidationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClinicWorkingHoursValidationJob> _logger;

    public ClinicWorkingHoursValidationJob(IServiceProvider serviceProvider, ILogger<ClinicWorkingHoursValidationJob> logger)
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
            .Include(a => a.Clinic)
            .ToListAsync(cancellationToken);

        var sentKeys = await AvailabilityValidation.GetRecentlyNotifiedKeysAsync(
            unitOfWork, [NotificationType.AppointmentOutsideWorkingHours], now, cancellationToken);

        var adminByClinic = await AvailabilityValidation.ResolveClinicAdminIdsAsync(
            unitOfWork, appointments.Select(a => a.Clinic), cancellationToken);

        var flagged = 0;

        foreach (var appointment in appointments)
        {
            if (AvailabilityValidation.IsWithinClinicWorkingHours(appointment.Clinic, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime))
                continue;

            flagged++;

            if (appointment.Clinic == null || !adminByClinic.TryGetValue(appointment.Clinic.Id, out var adminId))
                continue;

            if (!sentKeys.Add((adminId, NotificationType.AppointmentOutsideWorkingHours)))
                continue;

            try
            {
                await fcmService.SendToUserAsync(adminId, NotificationType.AppointmentOutsideWorkingHours, new()
                {
                    ["clinicName"] = appointment.Clinic.Name ?? "",
                    ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                    ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
                    ["appointmentId"] = appointment.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send outside-working-hours notification for appointment {AppointmentId}.", appointment.Id);
            }
        }

        _logger.LogInformation(
            "Clinic working hours validation: checked {AppointmentCount} upcoming appointments, flagged {FlaggedCount} outside clinic working hours.",
            appointments.Count, flagged);

        // Single commit: all notification rows added by the sends above in one transaction.
        await unitOfWork.SaveChangesAsync();
    }
}
