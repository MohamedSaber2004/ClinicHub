using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostsPagginated
{
    public class GetPostsQueryPagginatedHandler : IRequestHandler<GetPostsQueryPagginated, PagginatedResult<PostDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public GetPostsQueryPagginatedHandler(IUnitOfWork unitOfWork, SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
        }

        public async Task<PagginatedResult<PostDto>> Handle(GetPostsQueryPagginated request, CancellationToken cancellationToken)
        {
            var postsRepo = _unitOfWork.GetRepository<Post, Guid>();
            var usersRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            
            var query = postsRepo.GetAllAsync(null)
                .Join(usersRepo.GetAllAsync(null),
                    post => post.AuthorId,
                    user => user.Id,
                    (post, user) => new { post, user })
                .OrderByDescending(x => x.post.CreatedAt);

            return await query
                .Select(x => new PostDto(
                    x.post.Id,
                    x.post.Content,
                    x.post.AuthorId,
                    x.user.FullName,
                    x.user.ProfilePictureUrl ?? string.Empty,
                    x.post.CreatedAt,
                    x.post.Reactions.Count,
                    x.post.Comments.Count,
                    x.post.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList()
                ))
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
