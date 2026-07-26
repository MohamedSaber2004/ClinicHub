using ClinicHub.Application.Features.StaffDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctorSchedule
{
    public class GetStaffDoctorScheduleQuery : IRequest<DoctorScheduleDto>
    {
        public Guid DoctorId { get; set; }
        public DateTime? Date { get; set; }
    }
}
