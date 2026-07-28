using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateDoctorCommandValidator(
            IStringLocalizer<Messages> localizer,
            IUnitOfWork ctx,
            UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;

            RuleFor(v => v.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.DoctorMessages.NotFound.Value]));

            RuleFor(v => v.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));

            When(v => v.Email != null, () =>
            {
                RuleFor(v => v.Email)
                    .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]))
                    .MustAsync(async (email, ct) =>
                    {
                        var user = await userManager.FindByEmailAsync(email);
                        return user is null || user.IsDeleted;
                    }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]));
            });

            When(v => v.PhoneNumber != null, () =>
            {
                RuleFor(v => v.PhoneNumber)
                    .MustAsync(async (phone, ct) =>
                    {
                        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted, ct);
                        return user is null;
                    }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]));
            });

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

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.GetAllAsync(null).IgnoreQueryFilters().AnyAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
