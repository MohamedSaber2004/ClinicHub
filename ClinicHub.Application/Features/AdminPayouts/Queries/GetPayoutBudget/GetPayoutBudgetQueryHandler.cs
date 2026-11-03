using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBudget;

public class GetPayoutBudgetQueryHandler : IRequestHandler<GetPayoutBudgetQuery, PayoutBudgetResult>
{
    private readonly IPayoutService _payoutService;

    public GetPayoutBudgetQueryHandler(IPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    public async Task<PayoutBudgetResult> Handle(
        GetPayoutBudgetQuery request,
        CancellationToken cancellationToken) =>
        await _payoutService.GetBudgetAsync(cancellationToken);
}
