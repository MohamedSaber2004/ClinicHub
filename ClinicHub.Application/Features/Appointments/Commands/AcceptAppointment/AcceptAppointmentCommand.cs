using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.AcceptAppointment
{
    public class AcceptAppointmentCommand : IRequest<AppointmentAcceptanceResultDto>
    {
        public Guid AppointmentId { get; set; }
    }
}
