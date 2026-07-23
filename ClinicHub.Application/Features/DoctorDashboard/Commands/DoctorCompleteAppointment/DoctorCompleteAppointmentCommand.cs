using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorCompleteAppointment
{
    public class DoctorCompleteAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
