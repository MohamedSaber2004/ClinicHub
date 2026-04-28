using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Comments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Queries.GetAllCommentsPagginated
{
    public record GetAllCommentsPagginatedQuery(
        int PageNumber = PagginatedResult<CommentDto>.DefaultPageNumber, 
        int PageSize = PagginatedResult<CommentDto>.DefaultPageSize) : IRequest<PagginatedResult<CommentDto>>;
}
