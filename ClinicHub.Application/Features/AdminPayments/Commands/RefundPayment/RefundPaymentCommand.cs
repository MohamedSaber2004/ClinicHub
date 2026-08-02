using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Commands.RefundPayment;

public class RefundPaymentCommand : IRequest<bool>
{
    public Guid PaymentId { get; set; }
    public string? Reason { get; set; }
}
