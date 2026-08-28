using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayments;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads;

public static class AdsOrderProcessor
{
    public static async Task<CreateAdsOrderResponseDto> CreateOrderAsync(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        IStringLocalizer<Messages> localizer,
        Guid clinicId,
        Guid adPackageId,
        int durationDays,
        string? logoImageUrl,
        string? returnUrl,
        string? paymentMethod,
        CancellationToken cancellationToken)
    {
        var clinic = await unitOfWork.ClinicRepository.FindByKeyAsync(clinicId, cancellationToken);
        if (clinic == null)
            throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var package = await unitOfWork.GetRepository<AdPackage, Guid>().FindByKeyAsync(adPackageId, cancellationToken);
        if (package == null)
            throw new NotFoundException(localizer[LocalizationKeys.PaymentMessages.AdPackageNotFound.Value]);

        if (!package.IsActive)
            throw new BadRequestException(localizer[LocalizationKeys.PaymentMessages.AdPackageNotActive.Value]);

        if (durationDays <= 0 || durationDays % package.DurationDays != 0)
            throw new BadRequestException(localizer[LocalizationKeys.AdsMessages.InvalidDuration.Value]);

        if (!await IsEligibleForAdsAsync(unitOfWork, clinicId, cancellationToken))
            throw new ForbiddenException(localizer[LocalizationKeys.PaymentMessages.AdsNotEligible.Value]);

        var amount = package.Price * (durationDays / (decimal)package.DurationDays);

        var user = await unitOfWork.GetRepository<ApplicationUser, Guid>()
            .GetAllAsync(u => u.ClinicId == clinic.Id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        var userId = user?.Id ?? clinic.ClinicAdminId
            ?? throw new BadRequestException(localizer[LocalizationKeys.PaymentMessages.PayerUserNotFound.Value]);

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

        var resolvedMethod = PaymentMethodMapper.ToEnum(paymentMethod);
        WalletPaymentResultDto payResult;

        if (resolvedMethod == PaymentMethod.PaymobCreditCard)
        {
            payResult = await paymobService.InitiateCheckoutPaymentAsync(amount, "EGP", billing, cancellationToken, returnUrl);
        }
        else
        {
            payResult = await paymobService.InitiateWalletPaymentAsync(amount, "EGP", billing, billing.PhoneNumber, cancellationToken, returnUrl);
        }

        var advertisement = new Advertisement
        {
            ClinicId = clinic.Id,
            AdPackageId = package.Id,
            DurationDays = durationDays,
            AmountPaid = amount,
            Currency = "EGP",
            Status = AdvertisementStatus.PendingPayment,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(durationDays),
            ImageUrl = logoImageUrl
        };
        await unitOfWork.GetRepository<Advertisement, Guid>().AddAsync(advertisement);

        var payment = new ClinicHub.Domain.Entities.Payment(PaymentType.Ads, userId, clinic.Id, amount)
        {
            PaymobOrderId = payResult.OrderId
        };
        payment.MarkAsProcessing(payResult.RedirectUrl, PaymentMethodMapper.ToDbString(resolvedMethod));
        payment.SetManualReference(null, $"{package.Name} - {durationDays} days");

        await unitOfWork.PaymentRepository.AddAsync(payment);
        advertisement.PaymentId = payment.Id;

        await unitOfWork.SaveChangesAsync();

        return new CreateAdsOrderResponseDto
        {
            PaymentId = payment.Id,
            RefNumber = payment.RefNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            PaymobRedirectUrl = payResult.RedirectUrl,
            PaymobPaymentKey = payResult.PaymentKey,
            ImageUrl = advertisement.ImageUrl
        };
    }

    public static async Task<bool> IsEligibleForAdsAsync(IUnitOfWork unitOfWork, Guid clinicId, CancellationToken cancellationToken)
    {
        // ADS IS NOW INDEPENDENT FROM SUBSCRIPTION PLANS
        // Eligibility requires only that the clinic exists and is Active.
        // Previously: required Active subscription (Premium/Enterprise). Now: standalone.
        var clinic = await unitOfWork.ClinicRepository.FindByKeyAsync(clinicId, cancellationToken);
        return clinic != null && clinic.Status == ClinicStatus.Active;
    }
}
