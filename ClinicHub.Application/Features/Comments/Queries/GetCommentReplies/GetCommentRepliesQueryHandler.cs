using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Comments.DTOs;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Queries.GetCommentReplies
{
    public class GetCommentRepliesQueryHandler : IRequestHandler<GetCommentRepliesQuery, PagginatedResult<CommentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCommentRepliesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagginatedResult<CommentDto>> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Comment, Guid>();
            
            var query = repo.GetBy(c => c.ParentCommentId == request.CommentId)
                .OrderBy(c => c.CreatedAt);

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
