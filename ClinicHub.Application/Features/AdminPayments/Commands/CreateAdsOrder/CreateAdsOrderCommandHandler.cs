using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public async Task<CreateAdsOrderResponseDto> Handle(CreateAdsOrderCommand request, CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId, cancellationToken);
        if (clinic == null)
            throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var package = await _unitOfWork.GetRepository<AdPackage, Guid>().FindByKeyAsync(request.AdPackageId, cancellationToken);
        if (package == null)
            throw new NotFoundException(_localizer[LocalizationKeys.PaymentMessages.AdPackageNotFound.Value]);

        if (!package.IsActive)
            throw new BadRequestException(_localizer[LocalizationKeys.PaymentMessages.AdPackageNotActive.Value]);

        if (!await IsEligibleForAdsAsync(clinic.Id, cancellationToken))
            throw new ForbiddenException(_localizer[LocalizationKeys.PaymentMessages.AdsNotEligible.Value]);

        var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>()
            .GetAllAsync(u => u.ClinicId == clinic.Id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        var userId = user?.Id ?? clinic.ClinicAdminId
            ?? throw new BadRequestException(_localizer[LocalizationKeys.PaymentMessages.PayerUserNotFound.Value]);

        var names = user?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { clinic.Name ?? "Clinic", "Owner" };
        var billing = new PaymentBillingData
        {
            FirstName = names[0],
            LastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "Owner",
            Email = string.IsNullOrWhiteSpace(user?.Email) ? clinic.Email ?? "clinic@clinichub.com" : user.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(clinic.Phone) ? "01000000000" : clinic.Phone,
            City = "Egypt",
            Country = "EG",
            Street = clinic.Address ?? clinic.Name,
            Building = "NA",
            Apartment = "NA",
            Floor = "NA",
            PostalCode = "NA",
            State = "Egypt"
        };

        var walletResult = await _paymobService.InitiateWalletPaymentAsync(
            package.Price, "EGP", billing, billing.PhoneNumber, cancellationToken, request.ReturnUrl);

        var payment = new ClinicHub.Domain.Entities.Payment(PaymentType.Ads, userId, clinic.Id, package.Price)
        {
            PaymobOrderId = walletResult.OrderId
        };
        payment.MarkAsProcessing(walletResult.RedirectUrl, "paymob");
        payment.SetManualReference(null, $"{package.Name} - {request.DurationDays} days");

        await _unitOfWork.PaymentRepository.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return new CreateAdsOrderResponseDto
        {
            PaymentId = payment.Id,
            RefNumber = payment.RefNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            PaymobRedirectUrl = walletResult.RedirectUrl,
            PaymobPaymentKey = walletResult.PaymentKey
        };
    }

    private async Task<bool> IsEligibleForAdsAsync(Guid clinicId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var subscription = await _unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.ClinicId == clinicId && s.Status == SubscriptionStatus.Active && s.EndDate > now)
            .Include(s => s.Plan)
                .ThenInclude(p => p!.Permissions)
            .FirstOrDefaultAsync(cancellationToken);

        return subscription?.Plan != null
            && subscription.Plan.Permissions.Any(pp => pp.Permission == SubscriptionPermission.AdvancedReports);
    }
}
