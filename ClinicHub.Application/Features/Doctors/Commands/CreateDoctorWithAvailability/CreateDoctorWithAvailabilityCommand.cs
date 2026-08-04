using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Commands.CreateDoctorWithAvailability
{
    public class CreateDoctorWithAvailabilityCommand : IRequest<DoctorDto>
    {
        public Guid ClinicId { get; set; }
        public Guid SpecializationId { get; set; }
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public List<CreateDoctorAvailabilityDto> Availabilities { get; set; } = new();

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? DoctorImage { get; set; }
    }
}
