using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.AcceptBooking
{
    public class AcceptBookingCommand : IRequest<bool>
    {
        public Guid BookingId { get; set; }
    }
}
