using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.InitiateSubscriptionPayment
{
    public class InitiateSubscriptionPaymentCommand : IRequest<InitiateSubscriptionPaymentResponseDto>
    {
        public Guid PlanId { get; set; }
        public SubscriptionPlan Period { get; set; }
    }
}
