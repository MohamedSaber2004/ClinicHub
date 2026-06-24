using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommand : IRequest<AppointmentDto>
    {
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public AppointmentType AppointmentType { get; set; }

        public string PatientFullName { get; set; } = null!;
        public string PatientPhoneNumber { get; set; } = null!;
        public int PatientAge { get; set; }
        public Gender PatientGender { get; set; }
        public string Complaint { get; set; } = null!;
        public string? ChronicDiseases { get; set; }
    }
}
