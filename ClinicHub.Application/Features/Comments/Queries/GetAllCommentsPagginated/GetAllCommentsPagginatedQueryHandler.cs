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
            var usersRepo = _ctx.GetRepository<ApplicationUser, Guid>();

            var query = repo.GetAllAsync(null)
                .Join(usersRepo.GetAllAsync(null),
                    c => c.AuthorId,
                    u => u.Id,
                    (c, u) => new { c, u })
                .OrderByDescending(x => x.c.CreatedAt);

            return await query
                .Select(x => new CommentDto(
                    x.c.Id,
                    x.c.Content,
                    x.c.AuthorId,
                    x.u.FullName,
                    x.c.PostId,
                    x.c.ParentCommentId,
                    x.c.CreatedAt,
                    x.c.Reactions.Count,
                    x.c.Replies.Count,
                    x.c.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList()
                ))
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
