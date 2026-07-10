namespace ClinicHub.Application.Features.Auth.DTOs
{
    public record SignupResponseDto(
        Guid UserId,
        string Message,
        bool IsPendingApproval = true);
}
