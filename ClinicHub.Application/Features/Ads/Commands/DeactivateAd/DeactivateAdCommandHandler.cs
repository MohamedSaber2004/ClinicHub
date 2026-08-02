using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.DeactivateAd;

public class DeactivateAdCommandHandler : IRequestHandler<DeactivateAdCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<Messages> _localizer;

    public DeactivateAdCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<bool> Handle(DeactivateAdCommand request, CancellationToken cancellationToken)
    {
        var ad = await _unitOfWork.GetRepository<Advertisement, Guid>().FindByKeyAsync(request.Id, cancellationToken);
        if (ad == null)
            throw new NotFoundException(_localizer[LocalizationKeys.AdsMessages.NotFound.Value]);

        if (ad.Status is AdvertisementStatus.Deactivated or AdvertisementStatus.Expired)
            throw new BadRequestException(_localizer[LocalizationKeys.AdsMessages.AlreadyInactive.Value]);

        ad.Deactivate();
        _unitOfWork.GetRepository<Advertisement, Guid>().Update(ad);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
