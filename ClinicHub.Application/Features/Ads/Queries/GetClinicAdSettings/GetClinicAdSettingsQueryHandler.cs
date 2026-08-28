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
        // ADS INDEPENDENT: show settings for all Active clinics
        var clinicsWithSubscription = await _unitOfWork.ClinicRepository
            .GetAllAsync(c => c.Status == ClinicStatus.Active && !c.IsDeleted)
            .Select(c => new { ClinicId = c.Id, ClinicName = c.Name })
            .Distinct()
            .ToListAsync(cancellationToken);

        var result = new List<ClinicAdSettingsDto>();

        foreach (var clinic in clinicsWithSubscription)
        {
            var activeAdsCount = await _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllAsync(a => a.ClinicId == clinic.ClinicId && a.Status == AdvertisementStatus.Active)
                .CountAsync(cancellationToken);

            var settings = await _unitOfWork.GetRepository<ClinicAdSettings, Guid>()
                .GetAllAsync(s => s.ClinicId == clinic.ClinicId)
                .FirstOrDefaultAsync(cancellationToken);

            result.Add(new ClinicAdSettingsDto
            {
                ClinicId = clinic.ClinicId,
                ClinicName = clinic.ClinicName ?? string.Empty,
                MaxAds = settings?.MaxAds ?? 0,
                MaxImpressions = settings?.MaxImpressions ?? 0,
                ActiveAdsCount = activeAdsCount
            });
        }

        return result;
    }
}
