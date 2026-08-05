using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.CreateClinicAdOrder;

public class CreateClinicAdOrderCommandHandler : IRequestHandler<CreateClinicAdOrderCommand, CreateAdsOrderResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStringLocalizer<Messages> _localizer;

    public CreateClinicAdOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        ICurrentUserService currentUserService,
        IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _currentUserService = currentUserService;
        _localizer = localizer;
    }

    public Task<CreateAdsOrderResponseDto> Handle(CreateClinicAdOrderCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.CurrentClinicId != request.ClinicId)
            throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        return AdsOrderProcessor.CreateOrderAsync(
            _unitOfWork,
            _paymobService,
            _localizer,
            request.ClinicId,
            request.AdPackageId,
            request.DurationDays,
            request.LogoImageUrl,
            request.ReturnUrl,
            cancellationToken);
    }
}
