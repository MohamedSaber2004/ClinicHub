using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQuery : IRequest<GetAvailableSlotsResponse>
    {
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        public DateTime? Date { get; set; }
    }

    public class GetAvailableSlotsResponse
    {
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        public string? RequestedDate { get; set; }
        public List<DayAvailabilityDto> Days { get; set; } = new();
    }

    public class DayAvailabilityDto
    {
        public string DayOfWeek { get; set; } = null!;
        public WorkingHoursInfo? WorkingHours { get; set; }
        public List<TimeSlotDto> Slots { get; set; } = new();
    }

    public class WorkingHoursInfo
    {
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
    }
}
