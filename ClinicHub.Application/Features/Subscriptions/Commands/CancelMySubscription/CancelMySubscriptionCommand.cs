using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CancelMySubscription
{
    public record CancelMySubscriptionCommand : IRequest<bool>;
}
