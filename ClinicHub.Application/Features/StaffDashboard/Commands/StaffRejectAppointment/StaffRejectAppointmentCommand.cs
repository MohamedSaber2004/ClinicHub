using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffRejectAppointment
{
    public class StaffRejectAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
        public string? Reason { get; set; }
    }
}
