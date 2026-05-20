using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;
using System;

namespace ClinicHub.Domain.Entities
{
    public class Appointment : BaseEntity<Guid>
    {
        public Guid BookedByUserId { get; private set; }
        public ApplicationUser BookedByUser { get; private set; } = null!;

        public Guid DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;

        public Guid ClinicId { get; private set; }
        public Clinic Clinic { get; private set; } = null!;

        public DateTime AppointmentDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }     

        public AppointmentType AppointmentType { get; private set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;

        public string PatientFullName { get; private set; } = null!;
        public string PatientPhoneNumber { get; private set; } = null!;
        public int PatientAge { get; private set; }
        public Gender PatientGender { get; private set; }
        public string Complaint { get; private set; } = null!;
        public string? ChronicDiseases { get; private set; }

        public string? CancellationReason { get; private set; }

        private Appointment() { }

        public Appointment(
            Guid bookedByUserId,
            Guid doctorId,
            Guid clinicId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime,
            AppointmentType appointmentType,
            string patientFullName,
            string patientPhoneNumber,
            int patientAge,
            Gender patientGender,
            string complaint,
            string? chronicDiseases)
        {
            BookedByUserId = bookedByUserId;
            DoctorId = doctorId;
            ClinicId = clinicId;
            AppointmentDate = appointmentDate.Date;
            StartTime = startTime;
            EndTime = endTime;
            AppointmentType = appointmentType;
            PatientFullName = patientFullName;
            PatientPhoneNumber = patientPhoneNumber;
            PatientAge = patientAge;
            PatientGender = patientGender;
            Complaint = complaint;
            ChronicDiseases = chronicDiseases;
            Status = AppointmentStatus.Pending;
        }

        public void Confirm() => Status = AppointmentStatus.Confirmed;
        
        public void Cancel(string reason)
        {
            Status = AppointmentStatus.Cancelled;
            CancellationReason = reason;
        }
        
        public void Complete() => Status = AppointmentStatus.Completed;
    }
}
