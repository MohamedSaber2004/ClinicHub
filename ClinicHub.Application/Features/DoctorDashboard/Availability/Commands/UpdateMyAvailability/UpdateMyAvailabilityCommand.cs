using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.UpdateMyAvailability
{
    public class UpdateMyAvailabilityCommand : IRequest<AvailabilityDto>
    {
        public Guid Id { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? SlotDurationMinutes { get; set; }
    }
}
