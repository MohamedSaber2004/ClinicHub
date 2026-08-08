using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.RegisterFcmToken
{
    public sealed class RegisterFcmTokenCommandHandler : IRequestHandler<RegisterFcmTokenCommand, string>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RegisterFcmTokenCommandHandler(
            ICurrentUserService currentUserService,
            IFcmService fcmService,
            IStringLocalizer<Messages> localizer)
        {
            _currentUserService = currentUserService;
            _fcmService = fcmService;
            _localizer = localizer;
        }

        public async Task<string> Handle(RegisterFcmTokenCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated)
                return string.Empty;

            await _fcmService.RegisterTokenAsync(_currentUserService.UserId, request.FcmToken, request.DevicePlatform!.Value);

            return JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.AuthMessages.FcmTokenRegistered.Value]);
        }
    }
}
