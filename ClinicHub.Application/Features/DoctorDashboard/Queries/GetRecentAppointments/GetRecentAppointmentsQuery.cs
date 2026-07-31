using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetRecentAppointments
{
    public class GetRecentAppointmentsQuery : IRequest<IReadOnlyCollection<DoctorAppointmentDto>>
    {
        /// <summary>Maximum number of appointments to return. Defaults to 5.</summary>
        public int Limit { get; set; } = 5;
    }
}
