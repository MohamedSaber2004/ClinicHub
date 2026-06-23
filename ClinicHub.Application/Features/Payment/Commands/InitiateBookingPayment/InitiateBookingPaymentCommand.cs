using ClinicHub.Application.Features.Payment.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment
{
    public class InitiateBookingPaymentCommand : IRequest<BookingPaymentResponseDto>
    {
        public Guid ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
    }
}
