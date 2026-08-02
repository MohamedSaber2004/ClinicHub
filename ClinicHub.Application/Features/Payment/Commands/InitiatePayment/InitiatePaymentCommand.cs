using MediatR;
using ClinicHub.Application.Features.Payment.DTOs;

namespace ClinicHub.Application.Features.Payment.Commands.InitiatePayment;

public record InitiatePaymentCommand(Guid AppointmentId, string? ReturnUrl = null) : IRequest<InitiatePaymentResponseDto>;
