using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.DoctorDashboard.DTOs
{
    public class DoctorPatientDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public int TotalVisits { get; set; }
        public DateTime LastVisitDate { get; set; }
    }
}
