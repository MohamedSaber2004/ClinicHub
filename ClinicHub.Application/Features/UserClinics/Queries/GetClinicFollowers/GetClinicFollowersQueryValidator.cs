using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.UserClinics.Queries.GetClinicFollowers
{
    public class GetClinicFollowersQueryValidator : AbstractValidator<GetClinicFollowersQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClinicFollowersQueryValidator(IStringLocalizer<Messages> localizer, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));
        }

        private async Task<bool> ClinicExists(Guid id, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
        }
    }
}
