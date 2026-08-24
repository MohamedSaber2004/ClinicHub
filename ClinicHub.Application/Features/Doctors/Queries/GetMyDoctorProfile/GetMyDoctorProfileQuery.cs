using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Queries.GetMyDoctorProfile
{
    public class GetMyDoctorProfileQuery : IRequest<DoctorDto>
    {
    }
}
