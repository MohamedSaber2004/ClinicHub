using System;

namespace ClinicHub.Application.Features.Availability.DTOs
{
    public class TimeSlotDto
    {
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public bool IsAvailable { get; set; }
    }
}
