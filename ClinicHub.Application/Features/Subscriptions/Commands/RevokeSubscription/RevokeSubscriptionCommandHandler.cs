using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription
{
    /// <summary>
    /// Admin revoke delegates to the shared cancellation engine. The admin may
    /// override the refund amount (including 0 for cash/manual subscriptions).
    /// </summary>
    public class RevokeSubscriptionCommandHandler : IRequestHandler<RevokeSubscriptionCommand, CancelSubscriptionResponseDto>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ISubscriptionCancellationService _cancellationService;

        public RevokeSubscriptionCommandHandler(
            ICurrentUserService currentUserService,
            ISubscriptionCancellationService cancellationService)
        {
            _currentUserService = currentUserService;
            _cancellationService = cancellationService;
        }

        public async Task<CancelSubscriptionResponseDto> Handle(RevokeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            return await _cancellationService.CancelAsync(
                request.SubscriptionId,
                clinicIdScope: null,
                SubscriptionCancellationSource.Admin,
                _currentUserService.UserId,
                request.RefundAmount,
                request.Reason,
                requireOnlinePayment: false,
                cancellationToken);
        }
    }
}
