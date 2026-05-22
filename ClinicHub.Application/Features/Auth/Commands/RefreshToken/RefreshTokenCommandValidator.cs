using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandValidator(IStringLocalizer<Messages> localizer,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(BeValidRefreshToken).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.RefreshTokenInvalid.Value]));
        }

        private async Task<bool> BeValidRefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            var tokenEntity = await _unitOfWork.UserRefreshTokenRepository.GetFirstAsync(t => t.Token == refreshToken, cancellationToken);
            return tokenEntity != null && tokenEntity.IsValid;
        }
    }
}
