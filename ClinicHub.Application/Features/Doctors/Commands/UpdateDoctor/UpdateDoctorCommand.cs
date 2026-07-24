using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommand : IRequest<DoctorDto>
    {
        public Guid DoctorId { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool? IsActive { get; set; }
    }
}
