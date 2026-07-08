using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<PagginatedResult<UserDto>>
    {
        public int PageNumber { get; set; } = PagginatedResult<UserDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<UserDto>.DefaultPageSize;
        public string? SearchTerm { get; set; }
        public Guid? UserId { get; set; }
        public UserType? UserTypes { get; set; }
    }
}
