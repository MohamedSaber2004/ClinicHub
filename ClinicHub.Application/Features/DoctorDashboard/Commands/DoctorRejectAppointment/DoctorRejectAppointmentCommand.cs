using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorRejectAppointment
{
    public class DoctorRejectAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
        public string? Reason { get; set; }
    }
}
