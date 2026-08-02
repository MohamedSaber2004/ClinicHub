using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ads.Queries.GetMyAds;

public class GetMyAdsQueryHandler : IRequestHandler<GetMyAdsQuery, List<AdDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAdsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<AdDto>> Handle(GetMyAdsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.CurrentClinicId != request.ClinicId)
            return new List<AdDto>();

        var query = _unitOfWork.GetRepository<Advertisement, Guid>()
            .GetAllAsync(a => a.ClinicId == request.ClinicId);

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        var ads = await query
            .Include(a => a.AdPackage)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return ads.Select(a => new AdDto
        {
            Id = a.Id,
            PackageId = a.AdPackageId ?? Guid.Empty,
            PackageNameAr = a.AdPackage?.NameAr ?? a.AdPackage?.Name,
            DurationDays = a.DurationDays,
            Amount = a.AmountPaid,
            Currency = a.Currency,
            Status = a.Status,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            CreatedAt = a.CreatedAt
        }).ToList();
    }
}
