using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.UpdateAppointmentStatus
{
    /// <summary>
    /// Unified command for updating an appointment's status from the Doctor Dashboard.
    /// Status codes: 6 = Accepted (payment link sent), 2 = Rejected/Cancelled, 3 = Completed, 5 = No-show.
    /// </summary>
    public class UpdateAppointmentStatusCommand : IRequest<AppointmentAcceptanceResultDto?>
    {
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Target status: 6 = Accepted, 2 = Rejected/Cancelled, 3 = Completed, 5 = No-show.
        /// </summary>
        public int Status { get; set; }

        /// <summary>Optional notes or cancellation reason (used when Status = 2).</summary>
        public string? Notes { get; set; }
    }
}
