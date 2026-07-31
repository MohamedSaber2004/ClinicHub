using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.CreateMyAvailability
{
    public class CreateMyAvailabilityCommand : IRequest<AvailabilityDto>
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
    }
}
