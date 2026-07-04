using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs
{
    public class GetClinicAuditLogsQueryValidator : AbstractValidator<GetClinicAuditLogsQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClinicAuditLogsQueryValidator(IStringLocalizer<Messages> localizer, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]));

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeGreaterThanOrEqualToOne.Value]))
                .LessThanOrEqualTo(100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]));

            When(x => x.FromDate.HasValue && x.ToDate.HasValue, () =>
            {
                RuleFor(x => x.ToDate)
                    .GreaterThanOrEqualTo(x => x.FromDate)
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDateRange.Value]));
            });
        }

        private async Task<bool> ClinicExists(Guid id, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
        }
    }
}
