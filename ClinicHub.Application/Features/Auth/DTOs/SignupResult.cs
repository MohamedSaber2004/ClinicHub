namespace ClinicHub.Application.Features.Auth.DTOs
{
    public class SignupResult
    {
        public AuthResponseDto? AuthData { get; private set; }
        public SignupResponseDto? PendingData { get; private set; }

        public bool IsPendingApproval => PendingData != null;

        public static SignupResult Authenticated(AuthResponseDto authData) => new() { AuthData = authData };
        public static SignupResult Pending(SignupResponseDto pendingData) => new() { PendingData = pendingData };
    }
}
