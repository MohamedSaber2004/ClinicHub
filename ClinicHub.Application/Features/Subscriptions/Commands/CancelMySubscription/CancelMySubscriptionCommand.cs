using ClinicHub.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CancelMySubscription
{
    public record CancelMySubscriptionCommand : IRequest<CancelSubscriptionResponseDto>
    {
        public string? Reason { get; set; }
    }
}
