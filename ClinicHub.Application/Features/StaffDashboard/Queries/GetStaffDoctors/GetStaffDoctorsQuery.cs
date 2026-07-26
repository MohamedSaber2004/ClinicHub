using ClinicHub.Application.Features.StaffDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctors
{
    public class GetStaffDoctorsQuery : IRequest<List<DoctorBriefDto>>
    {
    }
}
