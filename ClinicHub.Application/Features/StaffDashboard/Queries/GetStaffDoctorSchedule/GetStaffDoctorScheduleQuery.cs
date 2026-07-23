using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctorSchedule
{
    public class GetStaffDoctorScheduleQuery : IRequest<List<AvailabilityDto>>
    {
        public Guid DoctorId { get; set; }
    }
}
