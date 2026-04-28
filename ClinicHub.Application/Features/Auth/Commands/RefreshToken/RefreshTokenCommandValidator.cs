using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;

namespace ClinicHub.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(BeValidRefreshToken).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.RefreshTokenInvalid.Value));
        }

        private async Task<bool> BeValidRefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            var tokenEntity = await _unitOfWork.UserRefreshTokenRepository.GetFirstAsync(t => t.Token == refreshToken, cancellationToken);
            return tokenEntity != null && tokenEntity.IsValid;
        }
    }
}
