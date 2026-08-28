using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ads.Queries.GetClinicAdSettings;

public class GetClinicAdSettingsQueryHandler : IRequestHandler<GetClinicAdSettingsQuery, List<ClinicAdSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClinicAdSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ClinicAdSettingsDto>> Handle(GetClinicAdSettingsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        var clinicsWithSubscription = await _unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > now)
            .Include(s => s.Clinic)
            .Where(s => s.Clinic != null && s.Clinic.Status == ClinicStatus.Active)
            .Select(s => new { s.ClinicId, ClinicName = s.Clinic!.Name })
            .Distinct()
            .ToListAsync(cancellationToken);

        var result = new List<ClinicAdSettingsDto>();

        foreach (var clinic in clinicsWithSubscription)
        {
            var activeAdsCount = await _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllAsync(a => a.ClinicId == clinic.ClinicId && a.Status == AdvertisementStatus.Active)
                .CountAsync(cancellationToken);

            result.Add(new ClinicAdSettingsDto
            {
                ClinicId = clinic.ClinicId,
                ClinicName = clinic.ClinicName ?? string.Empty,
                MaxAds = 0,
                MaxImpressions = 0,
                ActiveAdsCount = activeAdsCount
            });
        }

        return result;
    }
}
