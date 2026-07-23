using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription
{
    public class RevokeSubscriptionCommand : IRequest<bool>
    {
        public Guid SubscriptionId { get; set; }
    }
}
