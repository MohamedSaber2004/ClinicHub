using ClinicHub.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Queries.GetMyClinicSubscription
{
    public record GetMyClinicSubscriptionQuery : IRequest<SubscriptionDto?>;
}
