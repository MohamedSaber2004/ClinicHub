using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.AdminCreateSubscription
{
    public class AdminCreateSubscriptionCommand : IRequest<SubscriptionDto>
    {
        public Guid ClinicId { get; set; }
        public Guid PlanId { get; set; }
        public SubscriptionPlan Period { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal? Amount { get; set; }
    }
}
