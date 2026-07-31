using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.ReplaceWeeklyAvailability
{
    public class ReplaceWeeklyAvailabilityCommand : IRequest<List<AvailabilityDto>>
    {
        public List<AvailabilityDayInput> Days { get; set; } = new();
    }

    public class AvailabilityDayInput
    {
        public Guid? Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
    }
}
