using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetMyAppointments
{
    public class GetMyAppointmentsQuery : IRequest<List<MyAppointmentDto>>
    {
        /// <summary>
        /// Optional status filter: enum name in any case (Pending, Reserved, ...),
        /// enum number (0-7, where 0 is Pending), or "All"/empty for no filtering.
        /// Omit it to get every status.
        /// </summary>
        public string? Status { get; set; }
    }
}
