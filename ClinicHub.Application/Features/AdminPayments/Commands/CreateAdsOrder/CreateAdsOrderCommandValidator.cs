using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Commands.CreateAdsOrder;

public class CreateAdsOrderCommandValidator : AbstractValidator<CreateAdsOrderCommand>
{
    public CreateAdsOrderCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
    {
        RuleFor(v => v.ClinicId)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .MustAsync(async (id, ct) =>
                await ctx.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, ct))
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

        RuleFor(v => v.AdPackageId)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .MustAsync(async (id, ct) =>
                await ctx.GetRepository<AdPackage, Guid>().ExistsAsync(p => p.Id == id && !p.IsDeleted, ct))
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PaymentMessages.AdPackageNotFound.Value]));

        RuleFor(v => v.DurationDays)
            .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));

        RuleFor(v => v.ReturnUrl)
            .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
    }
}
