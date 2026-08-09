using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Auth.DTOs
{
    public record AuthResponseDto(
        string? AccessToken,
        string? RefreshToken,
        string FullName,
        string Email,
        string Roles,
        Guid id,
        Guid? ClinicId,
        Guid? DoctorId,
        string? ProfilePictureUrl,
        bool IsFreelanceDoctor = false,
        ClinicStatus? ClinicStatus = null,
        VerificationStatus? VerificationStatus = null,
        bool IsClinicSetupComplete = false);

    public record RefreshTokenResponseDto(
        string AccessToken,
        string RefreshToken);


    public record UserProfileDto(
        Guid Id,
        string FullName,
        string Email,
        Gender? Gender,
        string PhoneNumber,
        DateOnly? BirthDate,
        string? ProfilePictureUrl,
        LanguageCode Language,
        string Roles,
        bool IsFreelanceDoctor = false);
}
