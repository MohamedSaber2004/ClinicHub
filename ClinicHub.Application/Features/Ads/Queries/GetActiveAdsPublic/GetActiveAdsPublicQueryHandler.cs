using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ads.Queries.GetActiveAdsPublic;

public class GetActiveAdsPublicQueryHandler : IRequestHandler<GetActiveAdsPublicQuery, List<PublicAdDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveAdsPublicQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PublicAdDto>> Handle(GetActiveAdsPublicQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _unitOfWork.GetRepository<Advertisement, Guid>()
            .GetAllAsync(a => a.Status == AdvertisementStatus.Active && a.EndDate >= now)
            .Include(a => a.Clinic)
            .Include(a => a.AdPackage)
            .OrderByDescending(a => a.EndDate)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new PublicAdDto
            {
                Id = a.Id,
                ClinicId = a.ClinicId ?? Guid.Empty,
                ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                ClinicLogoUrl = a.Clinic != null ? a.Clinic.Logo : null,
                PackageId = a.AdPackageId ?? Guid.Empty,
                PackageNameAr = a.AdPackage != null ? (a.AdPackage.NameAr ?? a.AdPackage.Name) : null,
                Title = a.Title,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .ToListAsync(cancellationToken);
    }
}
