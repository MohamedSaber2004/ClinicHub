using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Queries.GetActiveAdPackages;

public class GetActiveAdPackagesQuery : IRequest<List<AdPackageDto>>
{
}
