using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutAccounts;

public class GetPayoutAccountsQuery : IRequest<List<PayoutAccountAdminDto>>
{
    public PayoutAccountStatus? Status { get; set; }
    public string? Search { get; set; }
}
