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
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(x => new ReactionDto(
                    x.Id,
                    x.AuthorId,
                    x.Type.ToString(),
                    x.CreatedAt)
                )
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
