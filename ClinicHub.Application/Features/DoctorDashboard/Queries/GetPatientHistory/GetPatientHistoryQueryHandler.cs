using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientHistory
{
    public class GetPatientHistoryQueryHandler : IRequestHandler<GetPatientHistoryQuery, PagginatedResult<PatientHistoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetPatientHistoryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<PatientHistoryDto>> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                return new PagginatedResult<PatientHistoryDto>(Array.Empty<PatientHistoryDto>(), 0);

            var query = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted
                    && a.BookedByUserId == request.PatientUserId)
                .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.StartTime);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Fetch latest assessment if available
            var assessmentRepo = _unitOfWork.GetRepository<PatientAssessment, Guid>();
            var assessment = await assessmentRepo.GetAllAsync(a => !a.IsDeleted && a.PatientId == request.PatientUserId
                    && (doctor == null || a.DoctorId == doctor.Id))
                .OrderByDescending(a => a.AssessmentDate)
                .ThenByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            PatientAssessmentDto? assessmentDto = null;
            if (assessment != null)
            {
                assessmentDto = new PatientAssessmentDto
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

            var dtos = items.Select(a => new PatientHistoryDto
            {
                AppointmentId = a.Id,
                AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                AppointmentType = a.AppointmentType,
                Status = a.Status,
                Complaint = a.Complaint,
                ChronicDiseases = a.ChronicDiseases,
                CancellationReason = a.CancellationReason,
                Assessment = assessmentDto
            }).ToList();

            return new PagginatedResult<PatientHistoryDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
