using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommand : IRequest<SubscriptionDto>
    {
        public Guid ClinicId { get; set; }
        public Guid PlanId { get; set; }
        public SubscriptionPlan Period { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? PaymentId { get; set; }
        public string? Notes { get; set; }
    }
}
