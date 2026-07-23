using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.RegisterWalkInPatient
{
    public class RegisterWalkInPatientCommand : IRequest<RegisterPatientResponseDto>
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public AppointmentType AppointmentType { get; set; }
        public string Complaint { get; set; } = null!;
        public string? ChronicDiseases { get; set; }
    }
}
