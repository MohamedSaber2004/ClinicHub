using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class NoShowSweepJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NoShowSweepJob> _logger;

    public NoShowSweepJob(IServiceProvider serviceProvider, ILogger<NoShowSweepJob> logger)
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

        var candidates = await unitOfWork.AppointmentRepository
            .GetAllAsync(a => (a.Status == AppointmentStatus.Accepted || a.Status == AppointmentStatus.Confirmed)
                && !a.CheckedInAt.HasValue
                && a.AppointmentDate <= now.Date)
            .Include(a => a.Clinic)
            .Include(a => a.Doctor)
            .ToListAsync(cancellationToken);

        foreach (var appointment in candidates)
        {
            var deadline = appointment.AppointmentDate.Add(appointment.EndTime).Add(NoShowJob.GracePeriod);
            if (now < deadline)
                continue;

            appointment.MarkNoShow();

            var parameters = new Dictionary<string, object>
            {
                ["patientName"] = appointment.PatientFullName,
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
                ["appointmentId"] = appointment.Id.ToString()
            };

            var recipients = new HashSet<Guid> { appointment.BookedByUserId, appointment.Doctor.UserId };
            if (appointment.Clinic?.ClinicAdminId.HasValue == true)
                recipients.Add(appointment.Clinic.ClinicAdminId.Value);

            foreach (var userId in recipients)
            {
                try
                {
                    await fcmService.SendToUserAsync(userId, NotificationType.AppointmentNoShow, parameters);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send no-show notification to user {UserId} for appointment {AppointmentId}.", userId, appointment.Id);
                }
            }

            _logger.LogInformation("Appointment {AppointmentId} marked as no-show by sweep.", appointment.Id);
        }

        await unitOfWork.SaveChangesAsync();
    }
}
