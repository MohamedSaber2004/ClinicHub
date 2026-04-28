using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostReactions
{
    public class GetPostReactionsQueryHandler : IRequestHandler<GetPostReactionsQuery, PagginatedResult<ReactionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPostReactionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagginatedResult<ReactionDto>> Handle(GetPostReactionsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Reaction, Guid>();
            
            var query = repo.GetAllAsync(r => r.PostId == request.PostId)
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new ReactionDto(
                    r.Id,
                    r.AuthorId,
                    r.Type.ToString(),
                    r.CreatedAt
                ))
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
