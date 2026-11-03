using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.UpdateStaff
{
    public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateStaffCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(v => v.StaffId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (command, id, ct) =>
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null) return false;
                    // Soft-deleted (deactivated) staff can only be touched to reactivate them.
                    if (user.IsDeleted && command.IsActive != true) return false;
                    var roles = await _userManager.GetRolesAsync(user);
                    return roles.Contains(nameof(UserType.Staff));
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.StaffMessages.NotFound.Value]));

            When(v => !string.IsNullOrWhiteSpace(v.FullName), () =>
            {
                RuleFor(v => v.FullName)
                    .MaximumLength(200).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
            });

            When(v => !string.IsNullOrWhiteSpace(v.PhoneNumber), () =>
            {
                RuleFor(v => v.PhoneNumber)
                    .MaximumLength(20).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]))
                    .MustAsync(async (command, phone, ct) =>
                    {
                        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && u.Id != command.StaffId && !u.IsDeleted, ct);
                        return user is null;
                    }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]));
            });

            RuleFor(v => v.Image)
                .Must(BeValidImageReference)
                .When(v => !string.IsNullOrWhiteSpace(v.Image))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));
        }

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

        // The staff flow stores raw upload filenames (e.g. "1_avatar.jpg"), so accept either
        // an absolute URL or a filename with a valid image extension — not URLs only.
        private static bool BeValidImageReference(string? image)
        {
            if (string.IsNullOrWhiteSpace(image)) return false;
            if (Uri.TryCreate(image, UriKind.Absolute, out _)) return true;
            var extension = Path.GetExtension(image);
            return !string.IsNullOrEmpty(extension)
                && AllowedImageExtensions.Contains(extension.ToLowerInvariant());
        }
    }
}
