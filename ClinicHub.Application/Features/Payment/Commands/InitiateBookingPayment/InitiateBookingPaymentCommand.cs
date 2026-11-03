using ClinicHub.Application.Features.Payment.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment
{
    public class InitiateBookingPaymentCommand : IRequest<BookingPaymentResponseDto>
    {
        public Guid ReservationId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReturnUrl { get; set; }
        public string? WalletPhoneNumber { get; set; }
    }
}
