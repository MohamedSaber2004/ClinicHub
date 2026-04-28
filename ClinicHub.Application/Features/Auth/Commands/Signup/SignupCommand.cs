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
        DateTime BirthDate,
        Gender Gender) : IRequest<AuthResponseDto>;
}
