using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class PatientAssessment : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid PatientId { get; private set; }
        public ApplicationUser? Patient { get; private set; }

        public Guid DoctorId { get; private set; }
        public Doctor? Doctor { get; private set; }

        public Guid? ClinicId { get; private set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic? Clinic { get; private set; }

        public Guid? AppointmentId { get; private set; }
        public Appointment? Appointment { get; private set; }

        public DateTime AssessmentDate { get; private set; } = DateTime.Today;

        // PERSONAL HISTORY
        public string PatientName { get; private set; } = string.Empty;
        public int? Age { get; private set; }
        public Gender? Gender { get; private set; }
        public string? Occupation { get; private set; }
        public string? Handedness { get; private set; } // Right, Left, Ambidextrous
        public string? SpecialHabits { get; private set; }
        public string? ContactNumber { get; private set; }
        public string? Address { get; private set; }
        public double? HeightCm { get; private set; }
        public double? WeightKg { get; private set; }
        public double? Bmi { get; private set; }

        // DIAGNOSIS
        public string? Diagnosis { get; private set; }

        // CHIEF COMPLAIN
        public string? ChiefComplaint { get; private set; }

        // PRESENT HISTORY
        public string? OnsetOfSymptoms { get; private set; }
        public string? DurationOfSymptoms { get; private set; }
        public string? CourseOfSymptoms { get; private set; }
        public string? MechanismOfInjury { get; private set; }
        public string? DiagnosticImagingAndLab { get; private set; }
        public string? PreviousTreatment { get; private set; }
        public DateTime? DateOfAdmission { get; private set; }
        public string? DateAndTypeOfSurgery { get; private set; }
        public string? SpecialPrecautions { get; private set; }

        // MEDICAL HISTORY
        public string? MedicalHistory { get; private set; }

        // FAMILY HISTORY
        public string? FamilyHistory { get; private set; }

        // TREATMENT PLAN (under family history)
        public string? TreatmentPlan { get; private set; }
        public string? Medications { get; private set; }
        public string? PhysicalTherapy { get; private set; }
        public string? TreatmentDuration { get; private set; }
        public string? FollowUpNotes { get; private set; }

        private PatientAssessment() { }

        public PatientAssessment(
            Guid patientId,
            Guid doctorId,
            Guid? clinicId,
            Guid? appointmentId,
            DateTime assessmentDate,
            string patientName,
            int? age,
            Gender? gender,
            string? occupation,
            string? handedness,
            string? specialHabits,
            string? contactNumber,
            string? address,
            double? heightCm,
            double? weightKg,
            double? bmi,
            string? diagnosis,
            string? chiefComplaint,
            string? onsetOfSymptoms,
            string? durationOfSymptoms,
            string? courseOfSymptoms,
            string? mechanismOfInjury,
            string? diagnosticImagingAndLab,
            string? previousTreatment,
            DateTime? dateOfAdmission,
            string? dateAndTypeOfSurgery,
            string? specialPrecautions,
            string? medicalHistory,
            string? familyHistory,
            string? treatmentPlan,
            string? medications,
            string? physicalTherapy,
            string? treatmentDuration,
            string? followUpNotes)
        {
            PatientId = patientId;
            DoctorId = doctorId;
            ClinicId = clinicId;
            AppointmentId = appointmentId;
            AssessmentDate = assessmentDate;
            PatientName = patientName;
            Age = age;
            Gender = gender;
            Occupation = occupation;
            Handedness = handedness;
            SpecialHabits = specialHabits;
            ContactNumber = contactNumber;
            Address = address;
            HeightCm = heightCm;
            WeightKg = weightKg;
            Bmi = bmi ?? CalculateBmi(heightCm, weightKg);
            Diagnosis = diagnosis;
            ChiefComplaint = chiefComplaint;
            OnsetOfSymptoms = onsetOfSymptoms;
            DurationOfSymptoms = durationOfSymptoms;
            CourseOfSymptoms = courseOfSymptoms;
            MechanismOfInjury = mechanismOfInjury;
            DiagnosticImagingAndLab = diagnosticImagingAndLab;
            PreviousTreatment = previousTreatment;
            DateOfAdmission = dateOfAdmission;
            DateAndTypeOfSurgery = dateAndTypeOfSurgery;
            SpecialPrecautions = specialPrecautions;
            MedicalHistory = medicalHistory;
            FamilyHistory = familyHistory;
            TreatmentPlan = treatmentPlan;
            Medications = medications;
            PhysicalTherapy = physicalTherapy;
            TreatmentDuration = treatmentDuration;
            FollowUpNotes = followUpNotes;
        }

        public void Update(
            DateTime assessmentDate,
            string patientName,
            int? age,
            Gender? gender,
            string? occupation,
            string? handedness,
            string? specialHabits,
            string? contactNumber,
            string? address,
            double? heightCm,
            double? weightKg,
            double? bmi,
            string? diagnosis,
            string? chiefComplaint,
            string? onsetOfSymptoms,
            string? durationOfSymptoms,
            string? courseOfSymptoms,
            string? mechanismOfInjury,
            string? diagnosticImagingAndLab,
            string? previousTreatment,
            DateTime? dateOfAdmission,
            string? dateAndTypeOfSurgery,
            string? specialPrecautions,
            string? medicalHistory,
            string? familyHistory,
            string? treatmentPlan,
            string? medications,
            string? physicalTherapy,
            string? treatmentDuration,
            string? followUpNotes)
        {
            AssessmentDate = assessmentDate;
            PatientName = patientName;
            Age = age;
            Gender = gender;
            Occupation = occupation;
            Handedness = handedness;
            SpecialHabits = specialHabits;
            ContactNumber = contactNumber;
            Address = address;
            HeightCm = heightCm;
            WeightKg = weightKg;
            Bmi = bmi ?? CalculateBmi(heightCm, weightKg);
            Diagnosis = diagnosis;
            ChiefComplaint = chiefComplaint;
            OnsetOfSymptoms = onsetOfSymptoms;
            DurationOfSymptoms = durationOfSymptoms;
            CourseOfSymptoms = courseOfSymptoms;
            MechanismOfInjury = mechanismOfInjury;
            DiagnosticImagingAndLab = diagnosticImagingAndLab;
            PreviousTreatment = previousTreatment;
            DateOfAdmission = dateOfAdmission;
            DateAndTypeOfSurgery = dateAndTypeOfSurgery;
            SpecialPrecautions = specialPrecautions;
            MedicalHistory = medicalHistory;
            FamilyHistory = familyHistory;
            TreatmentPlan = treatmentPlan;
            Medications = medications;
            PhysicalTherapy = physicalTherapy;
            TreatmentDuration = treatmentDuration;
            FollowUpNotes = followUpNotes;
        }

        public static double? CalculateBmi(double? heightCm, double? weightKg)
        {
            if (heightCm.HasValue && weightKg.HasValue && heightCm.Value > 0)
            {
                var heightMeters = heightCm.Value / 100.0;
                return Math.Round(weightKg.Value / (heightMeters * heightMeters), 1);
            }
            return null;
        }
    }
}
