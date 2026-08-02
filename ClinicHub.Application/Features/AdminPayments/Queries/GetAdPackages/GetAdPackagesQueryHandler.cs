using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdPackages;

public class GetAdPackagesQueryHandler : IRequestHandler<GetAdPackagesQuery, List<AdPackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAdPackagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<List<AdPackageDto>> Handle(GetAdPackagesQuery request, CancellationToken cancellationToken)
    {
        return _unitOfWork.GetRepository<AdPackage, Guid>()
            .GetAllAsync(p => !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .ProjectTo<AdPackageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
