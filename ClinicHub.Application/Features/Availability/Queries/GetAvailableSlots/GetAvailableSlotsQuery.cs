using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQuery : IRequest<List<TimeSlotDto>>
    {
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        public DateTime Date { get; set; }
    }
}
