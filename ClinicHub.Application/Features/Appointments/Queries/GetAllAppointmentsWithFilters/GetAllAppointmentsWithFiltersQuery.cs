using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAllAppointmentsWithFilters
{
    public class GetAllAppointmentsWithFiltersQuery : IRequest<PagginatedResult<AppointmentDto>>
    {
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AppointmentStatus? Status { get; set; }
        public string? PatientName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
