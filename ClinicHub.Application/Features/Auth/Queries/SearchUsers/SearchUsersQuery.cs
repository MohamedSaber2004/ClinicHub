using MediatR;
using ClinicHub.Application.Features.Auth.DTOs;

namespace ClinicHub.Application.Features.Auth.Queries.SearchUsers
{
    public class SearchUsersQuery : IRequest<List<UserSearchDto>>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }
}
