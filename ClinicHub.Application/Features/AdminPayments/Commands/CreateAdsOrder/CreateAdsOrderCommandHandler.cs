using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Ads;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Commands.CreateAdsOrder;

public class CreateAdsOrderCommandHandler : IRequestHandler<CreateAdsOrderCommand, CreateAdsOrderResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly IStringLocalizer<Messages> _localizer;

    public CreateAdsOrderCommandHandler(IUnitOfWork unitOfWork, IPaymobService paymobService, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _localizer = localizer;
    }

    public Task<CreateAdsOrderResponseDto> Handle(CreateAdsOrderCommand request, CancellationToken cancellationToken)
    {
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
