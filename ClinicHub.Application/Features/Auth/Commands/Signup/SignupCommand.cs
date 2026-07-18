using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.Signup
{
    public record SignupCommand(
        string FullName,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber,
        DateTime? BirthDate,
        Gender? Gender,
        string? ProfessionalPracticeCardImage,
        string? TaxCardImage,
        string? UnionIdCardImage,
        string? DoctorImage,
        TypeOfUserForRegisterFlow TypeOfUser,
        Guid? SpecializationId = null,
        string? Bio = null,
        int? YearsOfExperience = null,
        string? FcmToken = null,
        DevicePlatform? DevicePlatform = null) : IRequest<SignupResult>;
}
