using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class NoShowJob
{
    internal static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(30);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NoShowJob> _logger;

    public NoShowJob(IServiceProvider serviceProvider, ILogger<NoShowJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task MarkNoShowAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

        var appointment = await unitOfWork.AppointmentRepository
            .GetFirstWithIncluding(a => a.Id == appointmentId, a => a.Clinic!, a => a.Doctor)
            .FirstOrDefaultAsync(cancellationToken);
        if (appointment is null)
            return;
        if (appointment.Status is not (AppointmentStatus.Accepted or AppointmentStatus.Confirmed))
            return;
        if (appointment.CheckedInAt.HasValue)
            return;

        var deadline = appointment.AppointmentDate.Add(appointment.EndTime).Add(GracePeriod);
        if (DateTime.Now < deadline)
            return;

        appointment.MarkNoShow();
        await unitOfWork.SaveChangesAsync();

        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["patientName"] = appointment.PatientFullName,
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
                ["appointmentId"] = appointment.Id.ToString()
            };

            var recipients = new HashSet<Guid> { appointment.BookedByUserId };
            recipients.Add(appointment.Doctor.UserId);
            if (appointment.Clinic?.ClinicAdminId.HasValue == true)
                recipients.Add(appointment.Clinic.ClinicAdminId.Value);

            foreach (var userId in recipients)
                await fcmService.SendToUserAsync(userId, NotificationType.AppointmentNoShow, parameters);

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send no-show notifications for appointment {AppointmentId}.", appointmentId);
        }

        _logger.LogInformation("Appointment {AppointmentId} marked as no-show.", appointmentId);
    }
}
