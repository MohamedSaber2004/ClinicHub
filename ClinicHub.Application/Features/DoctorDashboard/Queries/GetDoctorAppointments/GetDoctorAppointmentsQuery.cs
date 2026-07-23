using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorAppointments
{
    public class GetDoctorAppointmentsQuery : IRequest<PagginatedResult<DoctorAppointmentDto>>
    {
        public AppointmentStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PatientName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
