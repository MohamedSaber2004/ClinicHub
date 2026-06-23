using ClinicHub.Application.Features.Payment.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.VerifyBookingPayment
{
    public class VerifyBookingPaymentCommand : IRequest<BookingPaymentResponseDto>
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; } = null!;
    }
}
