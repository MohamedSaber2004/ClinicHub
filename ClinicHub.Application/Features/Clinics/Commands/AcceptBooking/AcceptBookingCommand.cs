using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.AcceptBooking
{
    public class AcceptBookingCommand : IRequest<AppointmentAcceptanceResultDto>
    {
        public Guid BookingId { get; set; }
    }
}
