using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdPackages;

public class GetAdPackagesQuery : IRequest<List<AdPackageDto>>
{
}
