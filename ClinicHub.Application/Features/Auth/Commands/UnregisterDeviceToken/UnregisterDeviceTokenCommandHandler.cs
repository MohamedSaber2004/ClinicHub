using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicHub.Application.Features.Auth.Commands.UnregisterDeviceToken
{
    public sealed class UnregisterDeviceTokenCommandHandler : IRequestHandler<UnregisterDeviceTokenCommand, string>
    {
        private readonly IFcmService _fcmService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UnregisterDeviceTokenCommandHandler(
            IFcmService fcmService,
            IStringLocalizer<Messages> localizer)
        {
            _fcmService = fcmService;
            _localizer = localizer;
        }

        public async Task<string> Handle(UnregisterDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.FcmToken))
            {
                await _fcmService.UnregisterTokenAsync(request.FcmToken);
            }

            return JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.AuthMessages.LogoutSuccess.Value]);
        }
    }
}

