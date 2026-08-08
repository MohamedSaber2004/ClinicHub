using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class CancellationWindowJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CancellationWindowJob> _logger;

    public CancellationWindowJob(IServiceProvider serviceProvider, ILogger<CancellationWindowJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task CloseCancellationWindowAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

        var appointment = await unitOfWork.AppointmentRepository
            .GetAllAsync(a => a.Id == appointmentId)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment == null || appointment.Status is not (AppointmentStatus.Accepted or AppointmentStatus.Confirmed))
            return;

        await fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.CancellationWindowClosed, new()
        {
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["appointmentId"] = appointment.Id.ToString()
        });

        // Single commit: persist the notification row.
        await unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cancellation window closed for appointment {AppointmentId}.", appointmentId);
    }
}
