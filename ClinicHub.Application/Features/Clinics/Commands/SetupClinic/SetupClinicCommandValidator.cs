using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.SetupClinic
{
    public class SetupClinicCommandValidator : AbstractValidator<SetupClinicCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetupClinicCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _localizer = localizer;
            _ctx = ctx;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail])
                .MustAsync(BeUniqueClinicEmail).WithMessage(localizer[LocalizationKeys.ClinicMessages.EmailAlreadyExists]);

            RuleFor(x => x.Phone)
                .MaximumLength(11).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat])
                .MustAsync(BeUniqueClinicPhone).WithMessage(localizer[LocalizationKeys.ClinicMessages.PhoneAlreadyExists])
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MustAsync(SpecializationExists).WithMessage(localizer[LocalizationKeys.SpecializationMessages.NotFound]);

            RuleFor(x => x.Lat)
                .InclusiveBetween(-90, 90).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat]);

            RuleFor(x => x.Lng)
                .InclusiveBetween(-180, 180).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat]);
        }

        private async Task<bool> SpecializationExists(Guid specializationId, CancellationToken cancellationToken)
        {
            return await _ctx.SpecializationRepository.ExistsAsync(s => s.Id == specializationId, cancellationToken);
        }

        private async Task<bool> BeUniqueClinicEmail(string email, CancellationToken cancellationToken)
        {
            return !await _ctx.ClinicRepository.ExistsAsync(c => c.Email == email, cancellationToken);
        }

        private async Task<bool> BeUniqueClinicPhone(string? phone, CancellationToken cancellationToken)
        {
            return !await _ctx.ClinicRepository.ExistsAsync(c => c.Phone == phone, cancellationToken);
        }
    }
}
