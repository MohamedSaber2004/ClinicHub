using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetMyAppointments
{
    public class GetMyAppointmentsQuery : IRequest<List<MyAppointmentDto>>
    {
        public AppointmentStatus? Status { get; set; }
    }
}
