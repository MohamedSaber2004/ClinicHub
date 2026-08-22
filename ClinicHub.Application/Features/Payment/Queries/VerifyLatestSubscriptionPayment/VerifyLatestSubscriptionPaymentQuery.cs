using ClinicHub.Application.Features.Payment.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Queries.VerifyLatestSubscriptionPayment;

public class VerifyLatestSubscriptionPaymentQuery : IRequest<VerifySubscriptionPaymentResponseDto>
{
}
