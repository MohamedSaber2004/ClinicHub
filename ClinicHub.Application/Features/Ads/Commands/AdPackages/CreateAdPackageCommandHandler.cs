using AutoMapper;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class CreateAdPackageCommandHandler : IRequestHandler<CreateAdPackageCommand, AdPackageDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateAdPackageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AdPackageDto> Handle(CreateAdPackageCommand request, CancellationToken cancellationToken)
    {
        var maxSortOrder = await _unitOfWork.GetRepository<AdPackage, Guid>()
            .GetAllAsync(null)
            .Select(p => (int?)p.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var package = new AdPackage
        {
            Name = request.Name,
            NameAr = request.NameAr,
            Description = request.Description,
            DescriptionAr = request.DescriptionAr,
            Price = request.Price,
            DurationDays = request.DurationDays,
            IsActive = request.IsActive,
            MaxAds = request.MaxAds,
            MaxImpressions = request.MaxImpressions,
            SortOrder = maxSortOrder + 1
        };

        await _unitOfWork.GetRepository<AdPackage, Guid>().AddAsync(package);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AdPackageDto>(package);
    }
}
