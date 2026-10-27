using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientAssessment
{
    public class GetPatientAssessmentQuery : IRequest<PatientAssessmentDto>
    {
        public Guid PatientUserId { get; set; }
    }
}
