using ClinicHub.Application.Features.ClinicPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutAccount;

public class GetClinicPayoutAccountQuery : IRequest<ClinicPayoutAccountDto?>;
