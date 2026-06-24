using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
    {
        public AddUserCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.BirthDate).NotEmpty();
            RuleFor(x => x.Gender).IsInEnum();
            RuleFor(x => x.Role).NotEmpty()
                .Must(role => Enum.TryParse<UserType>(role, ignoreCase: true, out _))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidRole.Value]));
        }
    }
}
