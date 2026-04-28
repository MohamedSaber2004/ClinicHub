using ClinicHub.Application.Features.Auth.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Queries.GetUserProfile
{
    public record GetUserProfileQuery() : IRequest<UserProfileDto>;
}
