using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.CreateStaff
{
    public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClinicHubContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreateStaffCommandValidator(
            IStringLocalizer<Messages> localizer,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IClinicHubContext context)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _context = context;
            _localizer = localizer;
            PlanLimitResult? staffLimit = null;
            RuleFor(v => v.FullName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(200).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]))
                .MustAsync(async (email, ct) =>
                {
                    var user = await userManager.FindByEmailAsync(email);
                    return user == null;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]));

            RuleFor(v => v.PhoneNumber)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (phone, ct) =>
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted, ct);
                    return user is null;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]));

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MinimumLength(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MinLength.Value]));

            RuleFor(v => v)
                .MustAsync(async (_, ct) =>
                {
                    var clinicId = _currentUserService.CurrentClinicId;
                    if (clinicId == null)
                        return true;

                    staffLimit = await PlanLimitService.CanAddStaffAsync(_unitOfWork, userManager, _context, clinicId.Value, ct);
                    return staffLimit.Allowed;
                })
                .WithMessage(_ => JsonLocalizationProvider.GetLocalizedString(
                    staffLimit!.HasActiveSubscription
                        ? _localizer[LocalizationKeys.SubscriptionMessages.StaffLimitReached.Value, staffLimit.Limit ?? 0]
                        : _localizer[LocalizationKeys.SubscriptionMessages.NoActiveSubscription.Value]));
        }
    }
}
