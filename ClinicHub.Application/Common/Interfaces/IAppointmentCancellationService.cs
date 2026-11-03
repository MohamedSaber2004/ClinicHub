using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common.Interfaces;

/// <summary>
/// Shared implementation of the appointment cancellation flow.
/// Authorization must be performed by the caller before invoking <see cref="CancelAsync"/>.
/// </summary>
public interface IAppointmentCancellationService
{
    /// <summary>
    /// Cancels the appointment and returns any captured payment to the patient in full,
    /// reversing the clinic's accrued earning. A gateway failure does not block the
    /// cancellation: the refund is retried in the background.
    /// </summary>
    Task CancelAsync(Appointment appointment, string reason, CancellationToken cancellationToken);
}
