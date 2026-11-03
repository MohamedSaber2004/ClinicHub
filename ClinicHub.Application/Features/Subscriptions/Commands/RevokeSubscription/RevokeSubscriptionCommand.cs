using ClinicHub.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription
{
    public class RevokeSubscriptionCommand : IRequest<CancelSubscriptionResponseDto>
    {
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Optional refund override. Null → use the pro-rated quote;
        /// 0 → cancel with no refund (cash/manual); capped at the remaining paid amount.
        /// </summary>
        public decimal? RefundAmount { get; set; }

        /// <summary>Human-readable cancellation reason, stored on the subscription.</summary>
        public string? Reason { get; set; }
    }
}
