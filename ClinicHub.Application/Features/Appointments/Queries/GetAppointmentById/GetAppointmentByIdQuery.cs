using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAppointmentById
{
    public class GetAppointmentByIdQuery : IRequest<AppointmentDto>
    {
        public Guid Id { get; set; }
    }
}
