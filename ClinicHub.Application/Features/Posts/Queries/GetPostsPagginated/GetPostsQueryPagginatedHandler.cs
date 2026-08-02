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
        private readonly UserManager<ApplicationUser> _userManager;

        public GetPostsQueryPagginatedHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<PagginatedResult<PostDto>> Handle(GetPostsQueryPagginated request, CancellationToken cancellationToken)
        {
            var postsRepo = _unitOfWork.GetRepository<Post, Guid>();
            var usersRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            var doctorsRepo = _unitOfWork.DoctorRepository;

            var query = postsRepo.GetAllAsync(null)
                .Join(usersRepo.GetAllAsync(null),
                    post => post.AuthorId,
                    user => user.Id,
                    (post, user) => new { post, user })
                .GroupJoin(doctorsRepo.GetAllAsync(null),
                    x => x.user.Id,
                    doctor => doctor.UserId,
                    (x, doctors) => new { x.post, x.user, doctors })
                .SelectMany(
                    x => x.doctors.DefaultIfEmpty(),
                    (x, doctor) => new { x.post, x.user, doctor })
                .OrderByDescending(x => x.post.CreatedAt);

            var page = await query
                .Select(x => new
                {
                    x.post,
                    x.user,
                    IsFreelanceDoctor = x.doctor != null && x.doctor.IsFreelance
                })
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

            var roleLookup = new Dictionary<Guid, string?>();
            foreach (var author in page.Items.Select(x => x.user).DistinctBy(u => u.Id))
            {
                var roles = await _userManager.GetRolesAsync(author);
                roleLookup[author.Id] = roles.FirstOrDefault();
            }

            var items = page.Items
                .Select(x => new PostDto(
                    x.post.Id,
                    x.post.Content,
                    x.post.AuthorId,
                    x.user.FullName,
                    x.user.ProfilePictureUrl ?? string.Empty,
                    x.post.CreatedAt,
                    x.post.Reactions.Count,
                    x.post.Comments.Count,
                    x.IsFreelanceDoctor,
                    x.post.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList(),
                    roleLookup.GetValueOrDefault(x.post.AuthorId)
                ))
                .ToList();

            return new PagginatedResult<PostDto>(items, page.TotalCount, page.PageNumber, page.PageSize);
        }
    }
}
