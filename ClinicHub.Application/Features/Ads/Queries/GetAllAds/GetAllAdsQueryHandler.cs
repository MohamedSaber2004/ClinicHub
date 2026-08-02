using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ads.Queries.GetAllAds;

public class GetAllAdsQueryHandler : IRequestHandler<GetAllAdsQuery, PagginatedResult<AdminAdDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllAdsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagginatedResult<AdminAdDto>> Handle(GetAllAdsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Advertisement> query = _unitOfWork.GetRepository<Advertisement, Guid>()
            .GetAllAsync(null)
            .Include(a => a.Clinic)
            .Include(a => a.AdPackage);

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        var pageNumber = request.PageNumber < 1 ? PagginatedResult<AdminAdDto>.DefaultPageNumber : request.PageNumber;
        var pageSize = request.PageSize < 1 ? PagginatedResult<AdminAdDto>.DefaultPageSize
                     : request.PageSize > PagginatedResult<AdminAdDto>.MaxPageSize ? PagginatedResult<AdminAdDto>.MaxPageSize
                     : request.PageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AdminAdDto
            {
                Id = a.Id,
                ClinicId = a.ClinicId ?? Guid.Empty,
                ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                PackageId = a.AdPackageId ?? Guid.Empty,
                PackageNameAr = a.AdPackage != null ? (a.AdPackage.NameAr ?? a.AdPackage.Name) : null,
                DurationDays = a.DurationDays,
                Amount = a.AmountPaid,
                Currency = a.Currency,
                Status = a.Status,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagginatedResult<AdminAdDto>(items, totalCount, pageNumber, pageSize);
    }
}
