using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommand : IRequest<DoctorDto>
    {
        public Guid DoctorId { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool? IsActive { get; set; }
        public List<CreateDoctorAvailabilityDto> Availabilities { get; set; } = new();

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? DoctorImage { get; set; }
    }
}
