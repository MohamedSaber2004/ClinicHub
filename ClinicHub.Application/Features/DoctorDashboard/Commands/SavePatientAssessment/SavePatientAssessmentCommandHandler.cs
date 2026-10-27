using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.SavePatientAssessment
{
    public class SavePatientAssessmentCommandHandler : IRequestHandler<SavePatientAssessmentCommand, PatientAssessmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SavePatientAssessmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PatientAssessmentDto> Handle(SavePatientAssessmentCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            var assessmentRepo = _unitOfWork.GetRepository<PatientAssessment, Guid>();

            PatientAssessment? existing = null;
            if (request.Assessment.Id.HasValue && request.Assessment.Id.Value != Guid.Empty)
            {
                existing = await assessmentRepo.GetFirstAsync(a => a.Id == request.Assessment.Id.Value && !a.IsDeleted, cancellationToken);
            }
            else
            {
                existing = await assessmentRepo.GetAllAsync(a => !a.IsDeleted && a.PatientId == request.PatientUserId
                        && (doctor == null || a.DoctorId == doctor.Id))
                    .OrderByDescending(a => a.AssessmentDate)
                    .ThenByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            DateTime parseDate(string? d) => DateTime.TryParse(d, out var val) ? val : DateTime.Today;
            DateTime? parseNullableDate(string? d) => DateTime.TryParse(d, out var val) ? val : null;

            var assessmentDate = parseDate(request.Assessment.AssessmentDate);
            var dateOfAdmission = parseNullableDate(request.Assessment.DateOfAdmission);

            if (existing != null)
            {
                existing.Update(
                    assessmentDate,
                    request.Assessment.PatientName ?? existing.PatientName,
                    request.Assessment.Age ?? existing.Age,
                    request.Assessment.Gender ?? existing.Gender,
                    request.Assessment.Occupation,
                    request.Assessment.Handedness,
                    request.Assessment.SpecialHabits,
                    request.Assessment.ContactNumber,
                    request.Assessment.Address,
                    request.Assessment.HeightCm,
                    request.Assessment.WeightKg,
                    request.Assessment.Bmi,
                    request.Assessment.Diagnosis,
                    request.Assessment.ChiefComplaint,
                    request.Assessment.OnsetOfSymptoms,
                    request.Assessment.DurationOfSymptoms,
                    request.Assessment.CourseOfSymptoms,
                    request.Assessment.MechanismOfInjury,
                    request.Assessment.DiagnosticImagingAndLab,
                    request.Assessment.PreviousTreatment,
                    dateOfAdmission,
                    request.Assessment.DateAndTypeOfSurgery,
                    request.Assessment.SpecialPrecautions,
                    request.Assessment.MedicalHistory,
                    request.Assessment.FamilyHistory,
                    request.Assessment.TreatmentPlan,
                    request.Assessment.Medications,
                    request.Assessment.PhysicalTherapy,
                    request.Assessment.TreatmentDuration,
                    request.Assessment.FollowUpNotes);

                existing.MarkAsUpdated(_currentUserService.UserId.ToString() ?? "System");
                assessmentRepo.Update(existing);
            }
            else
            {
                var newAssessment = new PatientAssessment(
                    request.PatientUserId,
                    doctor?.Id ?? Guid.Empty,
                    doctor?.ClinicId ?? request.Assessment.ClinicId,
                    request.Assessment.AppointmentId,
                    assessmentDate,
                    request.Assessment.PatientName ?? "مريض",
                    request.Assessment.Age,
                    request.Assessment.Gender,
                    request.Assessment.Occupation,
                    request.Assessment.Handedness,
                    request.Assessment.SpecialHabits,
                    request.Assessment.ContactNumber,
                    request.Assessment.Address,
                    request.Assessment.HeightCm,
                    request.Assessment.WeightKg,
                    request.Assessment.Bmi,
                    request.Assessment.Diagnosis,
                    request.Assessment.ChiefComplaint,
                    request.Assessment.OnsetOfSymptoms,
                    request.Assessment.DurationOfSymptoms,
                    request.Assessment.CourseOfSymptoms,
                    request.Assessment.MechanismOfInjury,
                    request.Assessment.DiagnosticImagingAndLab,
                    request.Assessment.PreviousTreatment,
                    dateOfAdmission,
                    request.Assessment.DateAndTypeOfSurgery,
                    request.Assessment.SpecialPrecautions,
                    request.Assessment.MedicalHistory,
                    request.Assessment.FamilyHistory,
                    request.Assessment.TreatmentPlan,
                    request.Assessment.Medications,
                    request.Assessment.PhysicalTherapy,
                    request.Assessment.TreatmentDuration,
                    request.Assessment.FollowUpNotes);

                newAssessment.MarkAsCreated(_currentUserService.UserId.ToString() ?? "System");
                await assessmentRepo.AddAsync(newAssessment);
                existing = newAssessment;
            }

            await _unitOfWork.SaveChangesAsync();

            return new PatientAssessmentDto
            {
                Id = existing.Id,
                PatientId = existing.PatientId,
                DoctorId = existing.DoctorId,
                ClinicId = existing.ClinicId,
                AppointmentId = existing.AppointmentId,
                AssessmentDate = existing.AssessmentDate.ToString("yyyy-MM-dd"),
                PatientName = existing.PatientName,
                Age = existing.Age,
                Gender = existing.Gender,
                Occupation = existing.Occupation,
                Handedness = existing.Handedness,
                SpecialHabits = existing.SpecialHabits,
                ContactNumber = existing.ContactNumber,
                Address = existing.Address,
                HeightCm = existing.HeightCm,
                WeightKg = existing.WeightKg,
                Bmi = existing.Bmi,
                Diagnosis = existing.Diagnosis,
                ChiefComplaint = existing.ChiefComplaint,
                OnsetOfSymptoms = existing.OnsetOfSymptoms,
                DurationOfSymptoms = existing.DurationOfSymptoms,
                CourseOfSymptoms = existing.CourseOfSymptoms,
                MechanismOfInjury = existing.MechanismOfInjury,
                DiagnosticImagingAndLab = existing.DiagnosticImagingAndLab,
                PreviousTreatment = existing.PreviousTreatment,
                DateOfAdmission = existing.DateOfAdmission?.ToString("yyyy-MM-dd"),
                DateAndTypeOfSurgery = existing.DateAndTypeOfSurgery,
                SpecialPrecautions = existing.SpecialPrecautions,
                MedicalHistory = existing.MedicalHistory,
                FamilyHistory = existing.FamilyHistory,
                TreatmentPlan = existing.TreatmentPlan,
                Medications = existing.Medications,
                PhysicalTherapy = existing.PhysicalTherapy,
                TreatmentDuration = existing.TreatmentDuration,
                FollowUpNotes = existing.FollowUpNotes
            };
        }
    }
}
