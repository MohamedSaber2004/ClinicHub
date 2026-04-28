using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Comments.DTOs;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Queries.GetAllCommentsPagginated
{
    public class GetAllCommentsPagginatedQueryHandler : IRequestHandler<GetAllCommentsPagginatedQuery, PagginatedResult<CommentDto>>
    {
        private readonly IUnitOfWork _ctx;

        public GetAllCommentsPagginatedQueryHandler(IUnitOfWork ctx)
        {
            _ctx = ctx;
        }

        public async Task<PagginatedResult<CommentDto>> Handle(GetAllCommentsPagginatedQuery request, CancellationToken cancellationToken)
        {
            var repo = _ctx.GetRepository<Comment, Guid>();

            var query = repo.GetAllAsync(null)
                .OrderByDescending(c => c.CreatedAt);

            return await query
                .Select(c => new CommentDto(
                    c.Id,
                    c.Content,
                    c.AuthorId,
                    c.PostId,
                    c.ParentCommentId,
                    c.CreatedAt,
                    c.Reactions.Count,
                    c.Replies.Count,
                    c.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList()
                ))
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
