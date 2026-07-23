using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorPatients
{
    public class GetDoctorPatientsQuery : IRequest<PagginatedResult<DoctorPatientDto>>
    {
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
