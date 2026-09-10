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
            int patientAge,
            Gender patientGender,
            string complaint,
            string? chronicDiseases)
        {
            BookedByUserId = bookedByUserId;
            DoctorId = doctorId;
            ClinicId = clinicId;
            // Timezone-free: keep the calendar date only, Kind=Unspecified.
            AppointmentDate = new DateTime(appointmentDate.Year, appointmentDate.Month, appointmentDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
            StartTime = startTime;
            EndTime = endTime;
            AppointmentType = appointmentType;
            PatientFullName = patientFullName;
            PatientAge = patientAge;
            PatientGender = patientGender;
            Complaint = complaint;
            ChronicDiseases = chronicDiseases;

            Status = AppointmentStatus.Pending;
        }

        // NOTE: new booking requests always enter as Pending (set by the
        // constructor) so they land in the staff decision queue. Nothing may
        // move a request to Reserved on creation.
        // (<see cref="ExpiresAt"/> is retained only as deprecated storage.)

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
            AppointmentDate = new DateTime(appointmentDate.Year, appointmentDate.Month, appointmentDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
            StartTime = startTime;
            EndTime = endTime;
            Complaint = complaint;
            ChronicDiseases = chronicDiseases;
        }
    }
}
