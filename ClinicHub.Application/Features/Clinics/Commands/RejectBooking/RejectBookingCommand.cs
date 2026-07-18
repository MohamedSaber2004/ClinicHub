using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.RejectBooking
{
    public class RejectBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
        public string? Reason { get; set; }
    }
}
