using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Queries.GetMyAds;

public class GetMyAdsQuery : IRequest<List<AdDto>>
{
    public Guid ClinicId { get; set; }
    public AdvertisementStatus? Status { get; set; }
}
