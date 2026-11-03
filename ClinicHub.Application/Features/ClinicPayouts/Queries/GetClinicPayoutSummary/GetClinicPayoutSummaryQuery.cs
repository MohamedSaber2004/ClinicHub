using ClinicHub.Application.Features.ClinicPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutSummary;

public class GetClinicPayoutSummaryQuery : IRequest<ClinicPayoutSummaryDto>;
