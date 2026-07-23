using ClinicHub.Application.Features.Auth.DTOs;

namespace ClinicHub.Application.Common.Exceptions
{
    public sealed class ForbiddenException: Exception
    {
        public AuthResponseDto? AuthData { get; }

        public ForbiddenException()
           : base()
        {
        }

        public ForbiddenException(string message) :
            base(message)
        {
        }

        public ForbiddenException(string message, AuthResponseDto authData) :
            base(message)
        {
            AuthData = authData;
        }
    }
}
