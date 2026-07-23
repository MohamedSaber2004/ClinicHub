using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffApproveAppointment
{
    public class StaffApproveAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
