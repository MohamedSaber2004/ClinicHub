using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientHistory
{
    public class GetPatientHistoryQuery : IRequest<PagginatedResult<PatientHistoryDto>>
    {
        public Guid PatientUserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
