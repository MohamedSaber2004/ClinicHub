using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public LogoutCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<string> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var tokenEntity = await _unitOfWork.UserRefreshTokenRepository
                .GetFirstAsync(x => x.Token == request.RefreshToken, cancellationToken);

            if (tokenEntity != null)
            {
                tokenEntity.Revoke();
                _unitOfWork.UserRefreshTokenRepository.Update(tokenEntity);
                await _unitOfWork.SaveChangesAsync();
            }

            return _localizer[LocalizationKeys.AuthMessages.LogoutSuccess.Value];
        }
    }
}
