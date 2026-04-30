using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.ValidateGoogleAccessToken
{
    public class ValidateGoogleAccessTokenCommand: IRequest<ValidateGoogleAccessTokenResponse>
    {
        public string IdToken { get; set; } = null!;
    }
}
