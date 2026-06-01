using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Comments.Queries.GetCommentReactions
{
    public class GetCommentReactionsQueryHandler : IRequestHandler<GetCommentReactionsQuery, PagginatedResult<ReactionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCommentReactionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagginatedResult<ReactionDto>> Handle(GetCommentReactionsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Reaction, Guid>();
            
            var query = repo.GetAllAsync(r => r.CommentId == request.CommentId)
                .Join(_unitOfWork.GetRepository<ApplicationUser, Guid>().GetAllAsync(null),
                    r => r.AuthorId,
                    u => u.Id,
                    (r, u) => new { r, u })
                .OrderByDescending(x => x.r.CreatedAt);

            return await query
                .Select(x => new ReactionDto(
                    x.r.Id,
                    x.u.Id,
                    x.u.FullName,
                    x.r.Type.ToString(),
                    x.r.CreatedAt)
                )
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
