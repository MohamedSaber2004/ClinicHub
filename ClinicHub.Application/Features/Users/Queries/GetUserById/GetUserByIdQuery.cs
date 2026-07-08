using ClinicHub.Application.Features.Users.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
}
