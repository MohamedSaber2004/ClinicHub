using MediatR;
using ClinicHub.Application.Features.Payment.DTOs;
using System;

namespace ClinicHub.Application.Features.Payment.Commands.InitiatePayment;

public record InitiatePaymentCommand(Guid AppointmentId, string PhoneNumber) : IRequest<InitiatePaymentResponseDto>;
