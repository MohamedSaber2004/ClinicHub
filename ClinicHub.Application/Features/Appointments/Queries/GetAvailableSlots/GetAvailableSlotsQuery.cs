using ClinicHub.Application.Common.Models;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQuery : IRequest<List<TimeSlotDto>>
    {
        public Guid DoctorId { get; set; }
        public Guid ClinicId { get; set; }
        public DateTime Date { get; set; }
    }
}
