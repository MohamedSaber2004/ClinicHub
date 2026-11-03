using ClinicHub.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Queries.GetMyCancellationQuote
{
    public class GetMyCancellationQuoteQuery : IRequest<SubscriptionCancellationQuoteDto>
    {
    }
}
