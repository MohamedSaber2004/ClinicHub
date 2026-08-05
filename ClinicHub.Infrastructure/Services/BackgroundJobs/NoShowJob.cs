using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class NoShowJob
{
    private static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(30);

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

        var appointment = await unitOfWork.AppointmentRepository.GetByIdAsync(appointmentId);
        if (appointment.Status is not (AppointmentStatus.Accepted or AppointmentStatus.Confirmed))
            return;

        var deadline = appointment.AppointmentDate.Add(appointment.EndTime).Add(GracePeriod);
        if (DateTime.UtcNow < deadline)
            return;

        appointment.MarkNoShow();
        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Appointment {AppointmentId} marked as no-show.", appointmentId);
    }
}
