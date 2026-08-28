using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Ads.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.UpdateClinicAdSettings;

public class UpdateClinicAdSettingsCommandHandler : IRequestHandler<UpdateClinicAdSettingsCommand, ClinicAdSettingsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<Messages> _localizer;

    public UpdateClinicAdSettingsCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<ClinicAdSettingsDto> Handle(UpdateClinicAdSettingsCommand request, CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId, cancellationToken);
        if (clinic == null)
            throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var activeAdsCount = await _unitOfWork.GetRepository<Advertisement, Guid>()
            .GetAllAsync(a => a.ClinicId == request.ClinicId && a.Status == AdvertisementStatus.Active)
            .CountAsync(cancellationToken);

        return new ClinicAdSettingsDto
        {
            ClinicId = clinic.Id,
            ClinicName = clinic.Name ?? string.Empty,
            MaxAds = request.MaxAds,
            MaxImpressions = request.MaxImpressions,
            ActiveAdsCount = activeAdsCount
        };
    }
}
