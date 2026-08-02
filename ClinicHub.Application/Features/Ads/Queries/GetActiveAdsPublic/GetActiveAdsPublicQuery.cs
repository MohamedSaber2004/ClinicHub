using ClinicHub.Application.Features.Ads.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Queries.GetActiveAdsPublic;

public class GetActiveAdsPublicQuery : IRequest<List<PublicAdDto>>
{
}
