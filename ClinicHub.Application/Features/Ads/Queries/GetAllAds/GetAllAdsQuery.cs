using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Queries.GetAllAds;

public class GetAllAdsQuery : IRequest<PagginatedResult<AdminAdDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public AdvertisementStatus? Status { get; set; }
}
