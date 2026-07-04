using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorsByClinic
{
    public class GetDoctorsByClinicQuery : IRequest<List<DoctorDto>>
    {
        public Guid ClinicId { get; set; }
    }
}
