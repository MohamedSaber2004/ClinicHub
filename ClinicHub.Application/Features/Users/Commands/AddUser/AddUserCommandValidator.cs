using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
    {
        private readonly IUnitOfWork _ctx;

        public AddUserCommandValidator(
            IStringLocalizer<Messages> localizer,
            IUnitOfWork ctx,
            UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]))
                .MustAsync(async (email, ct) =>
                {
                    var user = await userManager.FindByEmailAsync(email);
                    return user is null || user.IsDeleted;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MinimumLength(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MinLength.Value]));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (phone, ct) =>
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted, ct);
                    return user is null;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]));

            RuleFor(x => x.Gender)
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]))
                .Must(role => role != UserType.None && role != UserType.SuperAdmin)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidRole.Value]));

            When(x => x.Role is UserType.ClinicOwner or UserType.Doctor or UserType.Staff, () =>
            {
                RuleFor(x => x.ClinicId)
                    .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                    .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));
            });

            When(x => x.Role is UserType.ClinicOwner or UserType.Doctor, () =>
            {
                RuleFor(x => x.SpecializationId)
                    .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            });
        }

        private async Task<bool> ClinicExists(Guid? clinicId, CancellationToken cancellationToken)
        {
            return clinicId.HasValue
                && await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId.Value, cancellationToken);
        }
    }
}
