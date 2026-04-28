using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Auth.DTOs
{
    public record AuthResponseDto(
        string? AccessToken,
        string? RefreshToken,
        string FullName,
        string Email,
        Guid id,
        string? VerificationCode);

    public record RefreshTokenResponseDto(
        string AccessToken,
        string RefreshToken);


    public record UserProfileDto(
        Guid Id,
        string FullName,
        string Email,
        Gender Gender,
        string PhoneNumber,
        DateTime BirthDate,
        string? ProfilePictureUrl,
        LanguageCode Language);
}
