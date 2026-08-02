using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class DeleteAdPackageCommandHandler : IRequestHandler<DeleteAdPackageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<Messages> _localizer;

    public DeleteAdPackageCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<bool> Handle(DeleteAdPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _unitOfWork.GetRepository<AdPackage, Guid>().FindByKeyAsync(request.Id, cancellationToken);
        if (package == null)
            throw new NotFoundException(_localizer[LocalizationKeys.AdsMessages.PackageNotFound.Value]);

        var hasAds = await _unitOfWork.GetRepository<Advertisement, Guid>()
            .ExistsAsync(a => a.AdPackageId == request.Id, cancellationToken);

        if (hasAds)
            throw new ConflictException(_localizer[LocalizationKeys.AdsMessages.PackageInUse.Value]);

        _unitOfWork.GetRepository<AdPackage, Guid>().Delete(package);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
