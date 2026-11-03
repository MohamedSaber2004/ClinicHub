using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientAssessment
{
    public class GetPatientAssessmentQueryHandler : IRequestHandler<GetPatientAssessmentQuery, PatientAssessmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetPatientAssessmentQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<PatientAssessmentDto> Handle(GetPatientAssessmentQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            var assessmentRepo = _unitOfWork.GetRepository<PatientAssessment, Guid>();

            var assessment = await assessmentRepo.GetAllAsync(a => !a.IsDeleted && a.PatientId == request.PatientUserId
                    && (doctor == null || a.DoctorId == doctor.Id))
                .OrderByDescending(a => a.AssessmentDate)
                .ThenByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (assessment != null)
            {
                return new PatientAssessmentDto
                {
                    Id = assessment.Id,
                    PatientId = assessment.PatientId,
                    DoctorId = assessment.DoctorId,
                    ClinicId = assessment.ClinicId,
                    AppointmentId = assessment.AppointmentId,
                    AssessmentDate = assessment.AssessmentDate.ToString("yyyy-MM-dd"),
                    PatientName = assessment.PatientName,
                    Age = assessment.Age,
                    Gender = assessment.Gender,
                    Occupation = assessment.Occupation,
                    Handedness = assessment.Handedness,
                    SpecialHabits = assessment.SpecialHabits,
                    ContactNumber = assessment.ContactNumber,
                    Address = assessment.Address,
                    HeightCm = assessment.HeightCm,
                    WeightKg = assessment.WeightKg,
                    Bmi = assessment.Bmi,
                    Diagnosis = assessment.Diagnosis,
                    ChiefComplaint = assessment.ChiefComplaint,
                    OnsetOfSymptoms = assessment.OnsetOfSymptoms,
                    DurationOfSymptoms = assessment.DurationOfSymptoms,
                    CourseOfSymptoms = assessment.CourseOfSymptoms,
                    MechanismOfInjury = assessment.MechanismOfInjury,
                    DiagnosticImagingAndLab = assessment.DiagnosticImagingAndLab,
                    PreviousTreatment = assessment.PreviousTreatment,
                    DateOfAdmission = assessment.DateOfAdmission?.ToString("yyyy-MM-dd"),
                    DateAndTypeOfSurgery = assessment.DateAndTypeOfSurgery,
                    SpecialPrecautions = assessment.SpecialPrecautions,
                    MedicalHistory = assessment.MedicalHistory,
                    FamilyHistory = assessment.FamilyHistory,
                    TreatmentPlan = assessment.TreatmentPlan,
                    Medications = assessment.Medications,
                    PhysicalTherapy = assessment.PhysicalTherapy,
                    TreatmentDuration = assessment.TreatmentDuration,
                    FollowUpNotes = assessment.FollowUpNotes
                };
            }

            // Fallback: prefill default values from user & appointments
            var user = await _userManager.FindByIdAsync(request.PatientUserId.ToString());
            var latestAppointment = await _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.BookedByUserId == request.PatientUserId && !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .FirstOrDefaultAsync(cancellationToken);

            var patientName = user?.FullName ?? latestAppointment?.PatientFullName ?? "مريض";
            int? age = null;
            if (user?.BirthDate.HasValue == true)
            {
                var today = DateTime.Today;
                var bDate = user.BirthDate.Value;
                age = today.Year - bDate.Year;
                if (bDate.Date > today.AddYears(-age.Value)) age--;
            }
            else if (latestAppointment != null && latestAppointment.PatientAge > 0)
            {
                age = latestAppointment.PatientAge;
            }

            var gender = user?.Gender ?? latestAppointment?.PatientGender;

            return new PatientAssessmentDto
            {
                Id = null,
                PatientId = request.PatientUserId,
                DoctorId = doctor?.Id,
                ClinicId = doctor?.ClinicId ?? latestAppointment?.ClinicId,
                AppointmentId = latestAppointment?.Id,
                AssessmentDate = DateTime.Today.ToString("yyyy-MM-dd"),
                PatientName = patientName,
                Age = age,
                Gender = gender,
                ContactNumber = user?.PhoneNumber,
                ChiefComplaint = latestAppointment?.Complaint,
                MedicalHistory = latestAppointment?.ChronicDiseases
            };
        }
    }
}
