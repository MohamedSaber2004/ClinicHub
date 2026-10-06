using MediatR;
using ClinicHub.Application.Features.Payment.DTOs;

namespace ClinicHub.Application.Features.Payment.Commands.InitiatePayment;

public record InitiatePaymentCommand(Guid AppointmentId, string? ReturnUrl = null, string? PaymentMethod = null, string? WalletPhoneNumber = null) : IRequest<InitiatePaymentResponseDto>;
