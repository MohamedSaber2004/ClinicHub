using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class UpdateAdPackageCommandHandler : IRequestHandler<UpdateAdPackageCommand, AdPackageDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<Messages> _localizer;

    public UpdateAdPackageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<AdPackageDto> Handle(UpdateAdPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _unitOfWork.GetRepository<AdPackage, Guid>().FindByKeyAsync(request.Id, cancellationToken);
        if (package == null)
            throw new NotFoundException(_localizer[LocalizationKeys.AdsMessages.PackageNotFound.Value]);

        package.Name = request.Name;
        package.NameAr = request.NameAr;
        package.Description = request.Description;
        package.DescriptionAr = request.DescriptionAr;
        package.Price = request.Price;
        package.DurationDays = request.DurationDays;
        package.IsActive = request.IsActive;

        _unitOfWork.GetRepository<AdPackage, Guid>().Update(package);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AdPackageDto>(package);
    }
}
