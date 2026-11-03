using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.VerifyPayoutAccount;

public class VerifyPayoutAccountCommand : IRequest<PayoutAccountAdminDto>
{
    public Guid AccountId { get; set; }
}
