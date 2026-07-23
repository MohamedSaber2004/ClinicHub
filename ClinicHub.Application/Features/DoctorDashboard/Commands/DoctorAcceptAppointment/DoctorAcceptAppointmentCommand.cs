using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorAcceptAppointment
{
    public class DoctorAcceptAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
