using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.DeleteAppointment
{
    public class DeleteAppointmentCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
