using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.RejectAppointment
{
    public class RejectAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
        public string? RejectionReason { get; set; }
    }
}
