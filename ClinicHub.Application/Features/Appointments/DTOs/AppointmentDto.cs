using ClinicHub.Domain.Enums;
using System;

namespace ClinicHub.Application.Features.Appointments.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid BookedByUserId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        
        public string AppointmentDate { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;

        public AppointmentType AppointmentType { get; set; }
        public AppointmentStatus Status { get; set; }

        public string PatientFullName { get; set; } = null!;
        public string PatientPhoneNumber { get; set; } = null!;
        public int PatientAge { get; set; }
        public Gender PatientGender { get; set; }
        public string Complaint { get; set; } = null!;
        public string? ChronicDiseases { get; set; }
        
        public string? CancellationReason { get; set; }
    }
}
