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

            //RuleFor(x => x.Dto.Name)
            //    .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            //    .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(BeUniqueClinicEmail).WithMessage(localizer[LocalizationKeys.ClinicMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(SpecializationExists);

            RuleFor(x => x.Phone)
                .MaximumLength(11).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(BeUniqueClinicPhone).WithMessage(localizer[LocalizationKeys.ClinicMessages.PhoneAlreadyExists.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));
            //RuleFor(x => x.Dto.Website)
            //    .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
            //    .When(x => !string.IsNullOrWhiteSpace(x.Dto.Website));

            //RuleFor(x => x.Dto.Logo)
            //    .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
            //    .When(x => !string.IsNullOrWhiteSpace(x.Dto.Logo));

            //RuleFor(x => x.Dto.NameAr)
            //    .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
            //    .When(x => !string.IsNullOrWhiteSpace(x.Dto.NameAr));

            //RuleFor(x => x.Dto.Description)
            //    .MaximumLength(1000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            //RuleFor(x => x.Dto.ArDescription)
            //    .MaximumLength(1000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            //RuleFor(x => x.Dto.Address)
            //    .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            //RuleFor(x => x.Dto.AddressAr)
            //    .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            //RuleFor(x => x.Dto.WorkingHours)
            //    .MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.OwnerName)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.OwnerEmail)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(async (email, ct) => !await EmailExists(email, ct)).WithMessage(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.OwnerPhone)
                .MaximumLength(11).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(async (phone, ct) => !await OwnerPhoneExists(phone, ct)).WithMessage(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.OwnerPhone));

            RuleFor(x => x.WorkingHoursStart)
                .LessThan(x => x.WorkingHoursEnd)
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value])
                .When(x => x.WorkingHoursStart.HasValue && x.WorkingHoursEnd.HasValue);

            RuleFor(x => x.Lat)
                .InclusiveBetween(-90, 90).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .When(x => x.Lat.HasValue);

            RuleFor(x => x.Lng)
                .InclusiveBetween(-180, 180).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .When(x => x.Lng.HasValue);

            RuleFor(x => x.DoctorSpecializationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(SpecializationExists);

            RuleFor(x => x.Bio)
                .MaximumLength(2000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Bio));

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .LessThanOrEqualTo(50).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]);

            RuleFor(x => x.WorkingDays)
                .Must(BeValidDayNames)
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidWorkingDays.Value])
                .When(x => x.WorkingDays != null && x.WorkingDays.Count > 0);
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
