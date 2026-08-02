using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Commands.CreateManualPayment;

public class CreateManualPaymentCommandValidator : AbstractValidator<CreateManualPaymentCommand>
{
    public CreateManualPaymentCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
    {
        RuleFor(v => v.PayerId)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .MustAsync(async (id, ct) =>
                await ctx.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, ct))
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

        RuleFor(v => v.Type)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]))
            .Must(t => t == PaymentType.Subscription || t == PaymentType.Ads)
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PaymentMessages.ManualTypeUnsupported.Value]));

        RuleFor(v => v.Amount)
            .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));

        RuleFor(v => v.Method)
            .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

        RuleFor(v => v.RefNumber)
            .MaximumLength(50).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

        RuleFor(v => v.Notes)
            .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
    }
}
