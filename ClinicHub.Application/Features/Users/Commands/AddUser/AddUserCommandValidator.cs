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
            RuleFor(x => x.Role).IsInEnum().Must(role => role != UserType.None && role != UserType.SuperAdmin);

            When(x => x.Role is UserType.ClinicOwner or UserType.Doctor or UserType.Staff, () =>
            {
                RuleFor(x => x.ClinicId).NotNull().NotEmpty();
            });
        }
    }
}
