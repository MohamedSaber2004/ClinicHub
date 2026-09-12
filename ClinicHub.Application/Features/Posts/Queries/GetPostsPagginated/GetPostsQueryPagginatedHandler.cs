using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Posts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Posts.Queries.GetPostsPagginated
{
    public class GetPostsQueryPagginatedHandler : IRequestHandler<GetPostsQueryPagginated, PagginatedResult<PostDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClinicHubContext _context;

        public GetPostsQueryPagginatedHandler(IUnitOfWork unitOfWork, IClinicHubContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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

            var authorIds = page.Items.Select(x => x.user.Id).Distinct().ToList();

            var roleIdToName = await _context.Roles
                .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken);

            var userRoleRows = await _context.UserRoles
                .Where(ur => authorIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);

            var roleLookup = userRoleRows
                .GroupBy(ur => ur.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g
                        .Select(ur => roleIdToName.GetValueOrDefault(ur.RoleId, string.Empty))
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Where(name => !name.Equals(nameof(UserType.User), StringComparison.OrdinalIgnoreCase))
                        .OrderBy(name => name)
                        .ToList());

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
                    roleLookup.GetValueOrDefault(x.post.AuthorId, Array.Empty<string>())
                ))
                .ToList();

            return new PagginatedResult<PostDto>(items, page.TotalCount, page.PageNumber, page.PageSize);
        }
    }
}
