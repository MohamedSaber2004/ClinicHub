using ClinicHub.Application.Localization;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.UpsertPayoutAccountAsAdmin;

public class UpsertPayoutAccountAsAdminCommandValidator : AbstractValidator<UpsertPayoutAccountAsAdminCommand>
{
    public UpsertPayoutAccountAsAdminCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
    {
        RuleFor(x => x.ClinicId)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .MustAsync(async (id, ct) =>
                await ctx.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, ct))
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .MaximumLength(200);

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .Must(BeValidNationalId)
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PayoutMessages.InvalidNationalId.Value]));

        RuleFor(x => x.BankCode)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .Must(PayoutBankCodes.IsValid)
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PayoutMessages.InvalidBankCode.Value]));

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
            .Must(BeValidAccountNumber)
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PayoutMessages.InvalidAccountNumber.Value]));

        RuleFor(x => x.BankTransactionType)
            .Must(type => string.IsNullOrWhiteSpace(type) || PayoutBankTransactionTypes.IsValid(type))
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PayoutMessages.InvalidTransactionType.Value]));
    }

    private static bool BeValidNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
            return false;

        return nationalId.All(char.IsDigit) && nationalId.Length == 14;
    }

    /// <summary>Paymob takes an account number, an IBAN or a card number, so letters are allowed.</summary>
    private static bool BeValidAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return false;

        var sanitized = new string(accountNumber.Where(c => char.IsLetterOrDigit(c)).ToArray());
        return sanitized.Length is >= 8 and <= 34;
    }
}
