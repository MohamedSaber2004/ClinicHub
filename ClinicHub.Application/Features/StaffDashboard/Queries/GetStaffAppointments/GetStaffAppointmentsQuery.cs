using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffAppointments
{
    public class GetStaffAppointmentsQuery : IRequest<PagginatedResult<StaffAppointmentDto>>
    {
        public AppointmentStatus? Status { get; set; }
        public DateTime? Date { get; set; }
        public string? PatientName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
