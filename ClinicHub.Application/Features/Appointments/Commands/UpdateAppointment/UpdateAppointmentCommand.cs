using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.UpdateAppointment
{
    public class UpdateAppointmentCommand : IRequest<AppointmentDto>
    {
        public Guid AppointmentId { get; set; }
        public UpdateAppointmentDto Dto { get; set; } = null!;
    }
}
