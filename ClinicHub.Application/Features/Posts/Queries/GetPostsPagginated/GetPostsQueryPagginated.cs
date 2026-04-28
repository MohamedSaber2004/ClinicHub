using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Application.Common.Models;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostsPagginated
{
    public record GetPostsQueryPagginated(
        int PageNumber = PagginatedResult<PostDto>.DefaultPageNumber,
        int PageSize = PagginatedResult<PostDto>.DefaultPageSize
    ) : IRequest<PagginatedResult<PostDto>>;
}
