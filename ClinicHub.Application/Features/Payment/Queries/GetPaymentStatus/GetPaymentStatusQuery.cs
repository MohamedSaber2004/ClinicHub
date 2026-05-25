using MediatR;
using ClinicHub.Application.Features.Payment.DTOs;
using System;

namespace ClinicHub.Application.Features.Payment.Queries.GetPaymentStatus;

public class GetPaymentStatusQuery : IRequest<PaymentStatusDto>
{
    public Guid AppointmentId { get; set; }
}
