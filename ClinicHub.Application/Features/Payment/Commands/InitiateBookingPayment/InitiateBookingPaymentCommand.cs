using ClinicHub.Application.Features.Payment.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment
{
    public class InitiateBookingPaymentCommand : IRequest<BookingPaymentResponseDto>
    {
        public Guid ReservationId { get; set; }
        /// <summary>Paymob method: "wallet" (PaymobWallet) or "card"/"creditcard" (PaymobCreditCard). Null = wallet (backward compat for /payments/initiate) or card for legacy booking. Handlers default gracefully.</summary>
        public string? PaymentMethod { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
