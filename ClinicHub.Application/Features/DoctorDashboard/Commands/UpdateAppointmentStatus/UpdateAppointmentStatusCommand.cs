using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.UpdateAppointmentStatus
{
    /// <summary>
    /// Unified command for updating an appointment's status from the Doctor Dashboard.
    /// Status codes: 1 = Accepted/Confirmed, 2 = Rejected/Cancelled, 3 = Completed.
    /// </summary>
    public class UpdateAppointmentStatusCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Target status: 1 = Accepted, 2 = Rejected/Cancelled, 3 = Completed.
        /// </summary>
        public int Status { get; set; }

        /// <summary>Optional notes or cancellation reason (used when Status = 2).</summary>
        public string? Notes { get; set; }
    }
}
