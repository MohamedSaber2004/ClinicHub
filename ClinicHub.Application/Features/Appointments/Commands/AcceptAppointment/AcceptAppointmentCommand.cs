using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.AcceptAppointment
{
    public class AcceptAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
