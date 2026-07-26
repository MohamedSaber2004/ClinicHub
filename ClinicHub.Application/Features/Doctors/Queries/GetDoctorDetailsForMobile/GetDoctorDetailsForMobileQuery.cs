using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorDetailsForMobile
{
    public class GetDoctorDetailsForMobileQuery : IRequest<DoctorDetailsForMobileDto>
    {
        public Guid DoctorId { get; set; }
    }
}
