using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetEligibleAdsClinics;

public class GetEligibleAdsClinicsQuery : IRequest<List<EligibleClinicDto>>
{
}
