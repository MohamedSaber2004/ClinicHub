using System;

namespace ClinicHub.Application.Features.Appointments.DTOs
{
    public class UpdateAppointmentDto
    {
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public string? Complaint { get; set; } = null!;
        public string? ChronicDiseases { get; set; }
    }
}
