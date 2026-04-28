using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostsPagginated
{
    public class GetPostsQueryPagginatedHandler : IRequestHandler<GetPostsQueryPagginated, PagginatedResult<PostDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPostsQueryPagginatedHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagginatedResult<PostDto>> Handle(GetPostsQueryPagginated request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<Post, Guid>();
            
            var query = repo.GetAllAsync(null)
                .OrderByDescending(p => p.CreatedAt);

            return await query
                .Select(p => new PostDto(
                    p.Id,
                    p.Content,
                    p.AuthorId,
                    p.CreatedAt,
                    p.Reactions.Count,
                    p.Comments.Count,
                    p.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList()
                ))
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
