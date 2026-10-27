using ClinicHub.Application.Common.Models;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBudget;

/// <summary>Live prepaid float Paymob debits our disbursements from. Run once per request — it is throttled to 5/minute.</summary>
public class GetPayoutBudgetQuery : IRequest<PayoutBudgetResult>
{
}
