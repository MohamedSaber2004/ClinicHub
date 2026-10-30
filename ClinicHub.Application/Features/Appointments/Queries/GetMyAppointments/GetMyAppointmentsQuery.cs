using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetMyAppointments
{
    public class GetMyAppointmentsQuery : IRequest<List<MyAppointmentDto>>
    {
        /// <summary>
        /// Optional status filter matching the mobile tabs: All (or empty), Pending,
        /// AwaitingPayment (same as Accepted), Confirmed, Completed, Cancelled, Rejected.
        /// Enum names in any case and enum numbers (0-7) are also accepted.
        /// </summary>
        public string? Status { get; set; }
    }
}
