using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Commands.CreateDoctorWithAvailability
{
    public class CreateDoctorWithAvailabilityCommand : IRequest<DoctorDto>
    {
        public Guid ClinicId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpecializationId { get; set; }
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public List<CreateDoctorAvailabilityDto> Availabilities { get; set; } = new();
    }
}
