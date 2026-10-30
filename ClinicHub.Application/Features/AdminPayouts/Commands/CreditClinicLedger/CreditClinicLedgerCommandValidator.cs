using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CreditClinicLedger;

public class CreditClinicLedgerCommandValidator : AbstractValidator<CreditClinicLedgerCommand>
{
    public CreditClinicLedgerCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
    {
        RuleFor(x => x.ClinicId)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .MustAsync(async (id, ct) =>
                await ctx.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, ct))
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
    }
}
