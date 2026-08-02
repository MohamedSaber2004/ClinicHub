using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Appointment : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid BookedByUserId { get; private set; }
        public ApplicationUser BookedByUser { get; private set; } = null!;

        public Guid DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;

        public Guid ClinicId { get; private set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic Clinic { get; private set; } = null!;

        public DateTime AppointmentDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public AppointmentType AppointmentType { get; private set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;

        public string PatientFullName { get; private set; } = null!;
        public string? PatientPhoneNumber { get; private set; }
        public int PatientAge { get; private set; }
        public Gender PatientGender { get; private set; }
        public string Complaint { get; private set; } = null!;
        public string? ChronicDiseases { get; private set; }

        public string? CancellationReason { get; private set; }

        public DateTime? ExpiresAt { get; private set; }
        public Guid? PaymentId { get; private set; }
        public Payment? Payment { get; private set; }

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
            string? patientPhoneNumber,
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

        public bool IsReservationExpired() =>
            ExpiresAt.HasValue && Status == AppointmentStatus.Reserved && DateTime.UtcNow >= ExpiresAt.Value;

        public void Reserve(int reservationTtlMinutes)
        {
            Status = AppointmentStatus.Reserved;
            ExpiresAt = DateTime.UtcNow.AddMinutes(reservationTtlMinutes);
        }

        public void ExpireReservation()
        {
            if (Status == AppointmentStatus.Reserved)
                Status = AppointmentStatus.Cancelled;
        }

        public void Confirm(Guid paymentId)
        {
            PaymentId = paymentId;
            Status = AppointmentStatus.Confirmed;
            ExpiresAt = null;
        }

        public void Cancel(string reason)
        {
            Status = AppointmentStatus.Cancelled;
            CancellationReason = reason;
            ExpiresAt = null;
        }

        public void Accept()
        {
            Status = AppointmentStatus.Accepted;
            ExpiresAt = null;
        }

        public void Reject(string? reason)
        {
            Status = AppointmentStatus.Rejected;
            CancellationReason = reason;
            ExpiresAt = null;
        }

        public void Complete() => Status = AppointmentStatus.Completed;

        public void CheckIn()
        {
            Status = AppointmentStatus.Confirmed;
            ExpiresAt = null;
        }

        public void MarkNoShow() => Status = AppointmentStatus.NoShow;

        public void Update(
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime,
            string complaint,
            string? chronicDiseases)
        {
            AppointmentDate = appointmentDate.Date;
            StartTime = startTime;
            EndTime = endTime;
            Complaint = complaint;
            ChronicDiseases = chronicDiseases;
        }
    }
}
