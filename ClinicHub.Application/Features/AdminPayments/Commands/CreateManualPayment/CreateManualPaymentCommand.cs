using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Commands.CreateManualPayment;

public class CreateManualPaymentCommand : IRequest<AdminPaymentDto>
{
    public Guid PayerId { get; set; }
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? RefNumber { get; set; }
    public string? Notes { get; set; }
}
