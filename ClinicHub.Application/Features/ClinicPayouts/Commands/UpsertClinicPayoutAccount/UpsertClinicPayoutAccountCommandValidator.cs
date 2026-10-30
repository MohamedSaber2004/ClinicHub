using ClinicHub.Application.Localization;
using ClinicHub.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.ClinicPayouts.Commands.UpsertClinicPayoutAccount;

public class UpsertClinicPayoutAccountCommandValidator : AbstractValidator<UpsertClinicPayoutAccountCommand>
{
    public UpsertClinicPayoutAccountCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .MaximumLength(200);

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .Must(BeValidNationalId)
            .WithMessage(localizer[LocalizationKeys.PayoutMessages.InvalidNationalId.Value]);

        RuleFor(x => x.BankCode)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .Must(PayoutBankCodes.IsValid)
            .WithMessage(localizer[LocalizationKeys.PayoutMessages.InvalidBankCode.Value]);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .Must(BeValidAccountNumber)
            .WithMessage(localizer[LocalizationKeys.PayoutMessages.InvalidAccountNumber.Value]);

        RuleFor(x => x.BankTransactionType)
            .Must(type => string.IsNullOrWhiteSpace(type) || PayoutBankTransactionTypes.IsValid(type))
            .WithMessage(localizer[LocalizationKeys.PayoutMessages.InvalidTransactionType.Value]);
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
