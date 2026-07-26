using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffCompleteAppointment
{
    public class StaffCompleteAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
