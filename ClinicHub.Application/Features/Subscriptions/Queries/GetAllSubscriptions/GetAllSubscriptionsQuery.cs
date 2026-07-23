using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Queries.GetAllSubscriptions
{
    public class GetAllSubscriptionsQuery : IRequest<PagginatedResult<SubscriptionDto>>
    {
        public SubscriptionStatus? Status { get; set; }
        public SubscriptionPlan? Plan { get; set; }
        public Guid? ClinicId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
