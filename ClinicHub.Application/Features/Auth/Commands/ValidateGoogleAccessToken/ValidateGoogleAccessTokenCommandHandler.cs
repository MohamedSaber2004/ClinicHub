using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.ValidateGoogleAccessToken
{
    public class ValidateGoogleAccessTokenCommandHandler : IRequestHandler<ValidateGoogleAccessTokenCommand, ValidateGoogleAccessTokenResponse>
    {
        private readonly IGoogleAuth _googleAuth;

        public ValidateGoogleAccessTokenCommandHandler(IGoogleAuth googleAuth)
        {
            _googleAuth = googleAuth;
        }

        public async Task<ValidateGoogleAccessTokenResponse> Handle(ValidateGoogleAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var isValid = await _googleAuth.ValidateGoogleTokenAsync(request.IdToken, cancellationToken);

            return new ValidateGoogleAccessTokenResponse
            {
                IsValidToken = isValid is not null
            };
        }
    }
}
