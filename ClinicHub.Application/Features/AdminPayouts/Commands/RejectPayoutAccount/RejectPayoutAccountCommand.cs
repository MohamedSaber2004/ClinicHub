using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.RejectPayoutAccount;

public class RejectPayoutAccountCommand : IRequest<PayoutAccountAdminDto>
{
    public Guid AccountId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
