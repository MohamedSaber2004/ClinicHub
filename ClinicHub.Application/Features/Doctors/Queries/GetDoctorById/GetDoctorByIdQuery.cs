using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorById
{
    public class GetDoctorByIdQuery : IRequest<DoctorDto>
    {
        public Guid Id { get; set; }
    }
}
