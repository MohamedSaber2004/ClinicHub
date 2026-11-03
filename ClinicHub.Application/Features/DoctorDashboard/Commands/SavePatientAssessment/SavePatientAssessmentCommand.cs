using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.SavePatientAssessment
{
    public class SavePatientAssessmentCommand : IRequest<PatientAssessmentDto>
    {
        public Guid PatientUserId { get; set; }
        public PatientAssessmentDto Assessment { get; set; } = null!;
    }
}
