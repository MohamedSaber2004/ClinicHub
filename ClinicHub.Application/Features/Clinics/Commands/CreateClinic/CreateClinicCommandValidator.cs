using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.CreateClinic
{
    public class CreateClinicCommandValidator : AbstractValidator<CreateClinicCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreateClinicCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
            _localizer = localizer;

            RuleFor(x => x.Dto)
                .NotNull().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Email)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(BeUniqueClinicEmail).WithMessage(localizer[LocalizationKeys.ClinicMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.Dto.SpecializationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(SpecializationExists);

            RuleFor(x => x.Dto.Phone)
                .MaximumLength(11).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(BeUniqueClinicPhone).WithMessage(localizer[LocalizationKeys.ClinicMessages.PhoneAlreadyExists.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Phone));

            RuleFor(x => x.Dto.Website)
                .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Logo)
                .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.NameAr)
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Description)
                .MaximumLength(1000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.ArDescription)
                .MaximumLength(1000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Address)
                .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.AddressAr)
                .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.WorkingHours)
                .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.OwnerName)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(100).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.OwnerEmail)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(async (email, ct) => !await EmailExists(email, ct)).WithMessage(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.Dto.OwnerPhone)
                .MaximumLength(11).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(async (phone, ct) => !await OwnerPhoneExists(phone, ct)).WithMessage(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.OwnerPhone));

            RuleFor(x => x.Dto.WorkingHoursStart)
                .LessThan(x => x.Dto.WorkingHoursEnd)
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value])
                .When(x => x.Dto.WorkingHoursStart.HasValue && x.Dto.WorkingHoursEnd.HasValue);

            RuleFor(x => x.Dto.WorkingDays)
                .Must(BeValidDayNames)
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidWorkingDays.Value])
                .When(x => x.Dto.WorkingDays != null && x.Dto.WorkingDays.Count > 0);
        }

        private static bool BeValidDayNames(List<DayOfWeek>? days)
        {
            if (days == null || days.Count == 0) return true;
            return days.All(d => Enum.IsDefined(typeof(DayOfWeek), d));
        }

        private async Task<bool> SpecializationExists(Guid specializationId, CancellationToken cancellationToken)
        {
            return await _ctx.SpecializationRepository.ExistsAsync(s => s.Id == specializationId, cancellationToken);
        }

        private async Task<bool> EmailExists(string email, CancellationToken cancellationToken)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        private async Task<bool> BeUniqueClinicEmail(string email, CancellationToken cancellationToken)
        {
            return !await _ctx.ClinicRepository.ExistsAsync(c => c.Email == email, cancellationToken);
        }

        private async Task<bool> BeUniqueClinicPhone(string? phone, CancellationToken cancellationToken)
        {
            return !await _ctx.ClinicRepository.ExistsAsync(c => c.Phone == phone, cancellationToken);
        }

        private async Task<bool> OwnerPhoneExists(string phone, CancellationToken cancellationToken)
        {
            return await _userManager.Users.AnyAsync(u => u.PhoneNumber == phone, cancellationToken);
        }
    }
}
