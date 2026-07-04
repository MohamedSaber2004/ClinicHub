using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
        public string CancellationReason { get; set; } = null!;
    }
}
