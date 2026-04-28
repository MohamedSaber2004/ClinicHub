using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Commands.VerifyUser
{
    public record VerifyUserCommand(string Email, string Code) : IRequest<string>;

    public class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyUserCommandValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .Length(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value))
                .Matches(@"^\d{6}$").WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value));

            RuleFor(x => x)
                .CustomAsync(async (x, context, ct) =>
                {
                    if(x.Email is null) 
                        context.AddFailure(nameof(x.Email), JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));

                    var user  = await _userManager.FindByEmailAsync(x.Email!);

                    if(user is null)
                    {
                        context.AddFailure(nameof(x.Email), JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.UserNotFound.Value));
                    } 

                    if (user?.VerificationCode != x.Code || user.VerificationCodeExpiry < DateTime.UtcNow)
                    {
                        context.AddFailure(nameof(x.Code), JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.InvalidVerificationCode.Value));
                    }
                });
        }
    }
}
