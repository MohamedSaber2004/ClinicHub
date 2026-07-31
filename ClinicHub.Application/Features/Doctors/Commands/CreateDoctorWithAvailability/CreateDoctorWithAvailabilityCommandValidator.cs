using ClinicHub.Application.Common.Services;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Commands.CreateDoctorWithAvailability
{
    public class CreateDoctorWithAvailabilityCommandValidator : AbstractValidator<CreateDoctorWithAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreateDoctorWithAvailabilityCommandValidator(
            IStringLocalizer<Messages> localizer,
            IUnitOfWork ctx,
            UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _localizer = localizer;
            PlanLimitResult? doctorLimit = null;

            RuleFor(v => v.FullName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]))
                .MustAsync(async (email, ct) =>
                {
                    var user = await userManager.FindByEmailAsync(email);
                    return user is null || user.IsDeleted;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]));

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MinimumLength(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MinLength.Value]));

            RuleFor(v => v.PhoneNumber)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (phone, ct) =>
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted, ct);
                    return user is null;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]));

            RuleFor(v => v.Gender)
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            RuleFor(v => v.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

            RuleFor(v => v.ClinicId)
                .MustAsync(async (clinicId, ct) =>
                {
                    doctorLimit = await PlanLimitService.CanAddDoctorAsync(_ctx, clinicId, ct);
                    return doctorLimit.Allowed;
                })
                .WithMessage(v => JsonLocalizationProvider.GetLocalizedString(
                    _localizer[LocalizationKeys.SubscriptionMessages.DoctorLimitReached.Value, doctorLimit!.Limit ?? 0]));

            RuleFor(v => v.SpecializationId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(SpecializationExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SpecializationMessages.NotFound.Value]));

            RuleFor(v => v.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));

            RuleForEach(v => v.Availabilities).ChildRules(av =>
            {
                av.RuleFor(x => x.DayOfWeek)
                    .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

                av.RuleFor(x => x.StartTime)
                    .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

                av.RuleFor(x => x.EndTime)
                    .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                    .GreaterThan(x => x.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]));

                av.RuleFor(x => x.SlotDurationMinutes)
                    .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]))
                    .LessThanOrEqualTo(480).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));
            });
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }

        private async Task<bool> SpecializationExists(Guid specializationId, CancellationToken cancellationToken)
        {
            return await _ctx.SpecializationRepository.ExistsAsync(s => s.Id == specializationId, cancellationToken);
        }
    }
}
